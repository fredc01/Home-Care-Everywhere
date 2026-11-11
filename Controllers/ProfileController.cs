using System.Security.Claims;
using HomeCarePH.Domain.Entities;
using HomeCarePH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeCarePH.Controllers;

[Authorize]
public class ProfileController(UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        return View(new ProfileViewModel { FullName = user.FullName, Email = user.Email ?? string.Empty, PhoneNumber = user.PhoneNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        if (!ModelState.IsValid) return View(model);
        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();
        var result = await userManager.SetPhoneNumberAsync(user, user.PhoneNumber);
        if (result.Succeeded && !string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            result = await userManager.SetEmailAsync(user, model.Email.Trim());
            if (result.Succeeded) result = await userManager.SetUserNameAsync(user, model.Email.Trim());
        }
        if (result.Succeeded) result = await userManager.UpdateAsync(user);
        if (result.Succeeded) TempData["ProfileMessage"] = "Your profile has been updated.";
        else foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }
}
