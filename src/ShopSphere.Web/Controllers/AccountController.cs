using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Features.Auth.DTOs;
using ShopSphere.Application.Features.Auth.Services;

namespace ShopSphere.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        // Store JWT in Secure Cookie for seamless MVC UI authentication
        Response.Cookies.Append("jwt_token", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in strict HTTPS production
            SameSite = SameSiteMode.Lax,
            Expires = result.ExpiresAt ?? DateTimeOffset.UtcNow.AddMinutes(120)
        });

        TempData["SuccessMessage"] = $"Welcome back, {result.FullName}!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequestDto model, CancellationToken cancellationToken = default)
    {
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
            return View(model);
        }

        var result = await _authService.RegisterAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        // Store JWT in cookie
        Response.Cookies.Append("jwt_token", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = result.ExpiresAt ?? DateTimeOffset.UtcNow.AddMinutes(120)
        });

        TempData["SuccessMessage"] = "Your account has been registered successfully!";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt_token");
        TempData["SuccessMessage"] = "You have been logged out successfully.";
        return RedirectToAction("Index", "Home");
    }
}
