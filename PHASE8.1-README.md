# HomeCarePH — Phase 8.1 Residents Management

## Delivered

- Authenticated Residents module with search, household filtering, archived-record filtering, and 12-record pagination.
- Create, view, and edit resident profiles with personal/contact information, date of birth, emergency contact, care requirements, and care notes.
- Archive/restore workflow using `IsActive` and `ArchivedAtUtc`; resident rows and linked care visits are never deleted.
- Resident-to-household association with server-side membership verification for both the current and target household.
- Resident detail care history showing related visits and caregivers where available.
- Responsive list cards, filter controls, detail panels, and mobile-friendly profile forms.
- PostgreSQL migration `20261111120000_AddResidentProfileManagement` for the new resident profile fields.

## Authorization boundary

Every Residents query is constrained by the signed-in user's `HouseholdMembership`. Detail, edit, archive, and restore load the resident by ID and then verify the resident's actual household membership server-side. Create verifies the submitted household ID against the current user's membership. Edit also verifies authorization for a requested household reassignment before changing `HouseholdId`; browser-supplied IDs are never treated as proof of access.

## Validation notes

The source-level checks confirm the new route manifest is valid, CSS braces are balanced, the migration is present, and the controller uses the existing access service on every resident mutation/read path. The sandbox does not include the .NET SDK, so run `dotnet build` and the migration/integration checks in CI or the deployment environment before publishing.

### Existing database recovery

If the app reports `42703: column r.IsActive does not exist`, the running database is still on the pre-8.1 schema. Stop or restart the app after applying the migration with `dotnet ef database update`, or run `scripts/phase8.1-resident-profile.sql` with `psql` against the same database. The migration and recovery script use `ADD COLUMN IF NOT EXISTS`, so either path is safe to repeat. Confirm the columns with `SELECT column_name FROM information_schema.columns WHERE table_name = 'Residents' ORDER BY ordinal_position;`, then reload `/Residents`.
