# HomeCarePH — Phase 8.3 Care Visit Management

Phase 8.3 adds authorized visit scheduling and tracking on the existing CareVisit relationship. Users can create, view, and edit visits; assign only an active caregiver assignment to the selected resident or household; record local start time, expected duration, instructions, visit notes, completion details, and status; and browse upcoming or historical visits.

Visit queries are constrained by the resident's household membership. The controller checks the resident's actual authorized household and the caregiver's active assignment server-side before every create or edit. Non-cancelled caregiver visits are checked for time overlap using each visit's expected duration; conflicting schedules are blocked on create/edit and visibly flagged on detail pages if legacy data already contains an overlap.

Migration `20261111123500_AddCareVisitScheduling` adds completion timestamps, duration, instructions, completion notes, and updated-at tracking. Run `dotnet build` and `dotnet ef database update` before publishing.
