using System.Security.Claims;
using HomeCarePH.Data;
using HomeCarePH.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Services;

public class HouseholdAccessService(ApplicationDbContext db)
{
    public async Task<HouseholdRole?> GetRoleAsync(ClaimsPrincipal principal, int householdId, CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return null;
        return await db.HouseholdMemberships
            .Where(x => x.HouseholdId == householdId && x.UserId == userId)
            .Select(x => (HouseholdRole?)x.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public static bool CanManageMembers(HouseholdRole role) => role is HouseholdRole.Owner or HouseholdRole.Admin;

    public static bool CanEditProperty(HouseholdRole role) => role is HouseholdRole.Owner or HouseholdRole.Admin or HouseholdRole.Member;
    public static bool CanManageCare(HouseholdRole role) => role is HouseholdRole.Owner or HouseholdRole.Admin or HouseholdRole.Member;
}
