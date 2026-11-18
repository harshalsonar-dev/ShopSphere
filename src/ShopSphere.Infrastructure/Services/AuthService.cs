using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Common.Interfaces;
using ShopSphere.Application.Features.Auth.DTOs;
using ShopSphere.Application.Features.Auth.Services;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.Data;

namespace ShopSphere.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // 1. Check if user already exists
        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser != null)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "A user with this email address already exists."
            };
        }

        // 2. Validate & determine role
        var requestedRoleName = string.IsNullOrWhiteSpace(request.Role) ? Role.CustomerRole : request.Role.Trim();
        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == requestedRoleName.ToLower(), cancellationToken);

        if (role == null)
        {
            // Default to Customer role if requested role not found
            role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == Role.CustomerRole, cancellationToken);
            if (role == null)
            {
                role = new Role(2, Role.CustomerRole);
                _dbContext.Roles.Add(role);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        // 3. Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 4. Create and persist user entity
        var user = new User(request.FullName, normalizedEmail, passwordHash, role.Id);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5. Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user, role.Name);

        return new AuthResultDto
        {
            Success = true,
            Message = "User registration successful.",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(120),
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = role.Name
        };
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // 1. Find user with role
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // 2. Verify password
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // 3. Generate token
        var roleName = user.Role?.Name ?? Role.CustomerRole;
        var token = _jwtTokenGenerator.GenerateToken(user, roleName);

        return new AuthResultDto
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(120),
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName
        };
    }
}
