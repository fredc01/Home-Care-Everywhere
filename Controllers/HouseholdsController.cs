using System.Security.Claims;
using System.Security.Cryptography;
using HomeCarePH.Data;
using HomeCarePH.Domain.Entities;
using HomeCarePH.Models;
using HomeCarePH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Controllers;

[Authorize]
public class HouseholdsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, HouseholdAccessService access) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var households = await db.HouseholdMemberships.Where(x => x.UserId == userId).OrderBy(x => x.Household.Name).Select(x => new HouseholdListItemViewModel { Id = x.HouseholdId, Name = x.Household.Name, Role = x.Role, MemberCount = x.Household.Memberships.Count }).ToListAsync(cancellationToken);
        return View(households);
    }

    public IActionResult Create() => View(new CreateHouseholdViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHouseholdViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var household = new Household { Name = model.Name.Trim() };
        household.Memberships.Add(new HouseholdMembership { UserId = userId, Role = HouseholdRole.Owner });
        db.Households.Add(household);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = household.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var role = await access.GetRoleAsync(User, id, cancellationToken);
        if (role is null) return Forbid();
        var household = await db.Households.Include(x => x.Memberships).ThenInclude(x => x.User).Include(x => x.Invitations).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (household is null) return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(new HouseholdDetailsViewModel { Household = household, CurrentRole = role.Value, Members = household.Memberships.OrderBy(x => x.Role).ToList(), PendingInvitations = household.Invitations.Where(x => x.IsPending).OrderByDescending(x => x.CreatedAtUtc).ToList(), Invite = new InviteMemberViewModel { HouseholdId = id } });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(InviteMemberViewModel model)
    {
        var role = await access.GetRoleAsync(User, model.HouseholdId);
        if (role is null) return Forbid();
        if (!HouseholdAccessService.CanManageMembers(role.Value)) return Forbid();
        if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id = model.HouseholdId });
        var email = model.Email.Trim().ToLowerInvariant();
        var existingMember = await db.HouseholdMemberships.AnyAsync(x => x.HouseholdId == model.HouseholdId && x.User.Email == email);
        if (existingMember) TempData["HouseholdMessage"] = "That person is already a household member.";
        else
        {
            var invitation = new HouseholdInvitation { HouseholdId = model.HouseholdId, Email = email, Role = model.Role, InvitedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!, Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), ExpiresAtUtc = DateTime.UtcNow.AddDays(7) };
            db.HouseholdInvitations.Add(invitation);
            await db.SaveChangesAsync();
            TempData["HouseholdMessage"] = $"Invitation created for {email}. Share this link: {Url.Action(nameof(AcceptInvitation), "Households", new { token = invitation.Token }, Request.Scheme)}";
        }
        return RedirectToAction(nameof(Details), new { id = model.HouseholdId });
    }

    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        var invitation = await db.HouseholdInvitations.Include(x => x.Household).SingleOrDefaultAsync(x => x.Token == token && x.AcceptedAtUtc == null);
        if (invitation is null || !invitation.IsPending) return View("InvitationInvalid");
        if (!User.Identity?.IsAuthenticated ?? true) return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(AcceptInvitation), new { token }) });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(userId);
        if (user?.Email?.Equals(invitation.Email, StringComparison.OrdinalIgnoreCase) != true) return View("InvitationMismatch", invitation);
        if (!await db.HouseholdMemberships.AnyAsync(x => x.HouseholdId == invitation.HouseholdId && x.UserId == userId)) db.HouseholdMemberships.Add(new HouseholdMembership { HouseholdId = invitation.HouseholdId, UserId = userId, Role = invitation.Role });
        invitation.AcceptedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = invitation.HouseholdId });
    }
}
