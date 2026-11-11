# HomeCarePH — Phase 8.2 Care Team Management

The Care Team module adds searchable and paginated caregiver profiles with contact information, role, skills, qualifications, specializations, availability, notes, and active/inactive status. Caregiver detail pages show active and ended assignments plus upcoming and previous visits visible through the signed-in user's authorized households.

Caregivers are intentionally not `ApplicationUser` records and have no household-login or platform-permission fields. The new `CaregiverAssignment` entity links a caregiver to an authorized household and optionally to an authorized resident. Assignment creation and ending validate the household and resident server-side; browser IDs are never treated as access proof. Existing `CareVisit` records remain the source of visit history and are filtered through the resident's household membership.

The migration `20261111122500_AddCareTeamManagement` adds caregiver profile columns and the assignment table. The model snapshot has been synchronized so `dotnet ef database update` will not report pending model changes. Run `dotnet build` and `dotnet ef database update` in the deployment environment, then verify create/edit, inactive filtering, assignment ending, and visit visibility across two household memberships.

If the app reports `42P01: relation "CaregiverAssignments" does not exist`, the running database is still on the pre-8.2 schema. Run `dotnet ef database update` from the deployed source, or apply `scripts/phase8.2-care-team.sql` with `psql` against the same database and restart the app. The recovery script is idempotent and covers both the caregiver profile columns and assignment table.
