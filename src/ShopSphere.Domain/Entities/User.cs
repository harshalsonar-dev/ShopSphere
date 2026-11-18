using ShopSphere.Domain.Common;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int RoleId { get; private set; }

    // Navigation properties
    public Role Role { get; private set; } = null!;
    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    protected User() { }

    public User(string fullName, string email, string passwordHash, int roleId)
    {
        SetFullName(fullName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        AssignRole(roleId);
    }

    public User(int id, string fullName, string email, string passwordHash, int roleId)
        : this(fullName, email, passwordHash, roleId)
    {
        Id = id;
    }

    public void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty.");

        FullName = fullName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");

        if (!email.Contains('@') || !email.Contains('.'))
            throw new DomainException("Invalid email format.");

        Email = email.Trim().ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRole(int roleId)
    {
        if (roleId <= 0)
            throw new DomainException("Invalid role identifier.");

        RoleId = roleId;
        UpdatedAt = DateTime.UtcNow;
    }
}
