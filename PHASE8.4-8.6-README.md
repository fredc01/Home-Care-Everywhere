# HomeCarePH — Phase 8.4–8.6 Domain, Access, and Acceptance

## Delivered

- Household membership, resident, caregiver, and care-visit concepts remain separate. Residents and caregivers are not implicitly platform accounts.
- Resident, caregiver, assignment, and visit reads are constrained to households visible through the signed-in user’s `HouseholdMembership`; browser-supplied IDs are rechecked against those relationships on every mutation.
- Care-management writes require an Owner, Admin, or Member role. Viewer memberships can read authorized records but cannot create or edit resident, caregiver, assignment, or visit data.
- Residents are archived with `IsActive`/`ArchivedAtUtc`; linked visits remain intact for historical continuity.
- Dashboard care counts and household-dependent summaries are now membership-scoped and link to the corresponding filtered module pages.
- Residents, Care Team, and Visits are real sidebar routes with dynamic active-state styling. Empty states, validation, conflict warnings, and responsive cards/forms are retained.
- Caregiver overlap checks remain server-side and are now also surfaced on visit list rows when legacy conflicting data exists. New or edited conflicting visits are rejected.
- Cookie authentication uses `SameSite=None` and `Secure` for HTTPS embedded preview access.

## Database and migration evidence

The Phase 8.1, 8.2, and 8.3 EF Core migrations remain in sequence after the existing Phase 7 migrations, and the model snapshot includes the resident profile, caregiver assignment, and visit scheduling fields. Startup continues to call `Database.MigrateAsync()`; no destructive data migration was added. Archived residents and existing visits are preserved.

The sandbox image does not include the .NET SDK, so `dotnet build` and `dotnet ef database update` must be run in CI/deployment where the project SDK and PostgreSQL connection are available.

## Acceptance mapping

- Residents: `/Residents` supports create, view, edit, household association, archive/restore, search, filtering, empty states, and historical visit display.
- Care Team: `/Caregivers` supports create, view, edit, active/inactive status, skills/qualifications/specializations/availability, household or resident assignments, and assignment ending.
- Care Visits: `/CareVisits` supports scheduling, history, resident/caregiver assignment, status, notes, completion details, pagination, conflict prevention, and visible conflict flags.
- Authorization: server-side membership and writable-role checks cover detail queries and write operations; sensitive care fields are not placed in query strings or diagnostic logs.
