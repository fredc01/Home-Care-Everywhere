using System.Text;
using HomeCarePH.Domain.Entities;
using HomeCarePH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace HomeCarePH.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AccountController> logger) : Controller
{
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null) => View(new RegisterViewModel { });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }
        AddErrors(result);
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return RedirectToLocal(model.ReturnUrl);
        if (result.IsLockedOut) ModelState.AddModelError(string.Empty, "Your account is temporarily locked. Try again later.");
        else ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
        return View(model);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null) return RedirectToAction(nameof(ForgotPasswordConfirmation));
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, code, email = user.Email }, Request.Scheme);
        logger.LogInformation("Password reset requested for {Email}. Reset link: {ResetUrl}", user.Email, resetUrl);
        TempData["ResetLink"] = resetUrl;
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation() => View();

    [AllowAnonymous]
    public IActionResult ResetPassword(string? userId, string? code, string? email)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code)) return RedirectToAction(nameof(ForgotPasswordConfirmation));
        return View(new ResetPasswordViewModel { UserId = userId, Code = code, Email = email ?? string.Empty });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user is null) return RedirectToAction(nameof(ResetPasswordConfirmation));
        IdentityResult result;
        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            result = await userManager.ResetPasswordAsync(user, token, model.Password);
        }
        catch (FormatException) { result = IdentityResult.Failed(new IdentityError { Description = "The reset link is invalid or expired." }); }
        if (result.Succeeded) return RedirectToAction(nameof(ResetPasswordConfirmation));
        AddErrors(result);
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation() => View();

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl) => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home")!;
    private void AddErrors(IdentityResult result) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); }
}
