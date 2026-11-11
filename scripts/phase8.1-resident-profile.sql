-- HomeCarePH Phase 8.1 recovery script
-- Run against the same PostgreSQL database configured by ConnectionStrings__DefaultConnection.
-- All statements are idempotent and safe to re-run.

ALTER TABLE "Residents" ADD COLUMN IF NOT EXISTS "ArchivedAtUtc" timestamp with time zone NULL;
ALTER TABLE "Residents" ADD COLUMN IF NOT EXISTS "CareNotes" character varying(4000) NULL;
ALTER TABLE "Residents" ADD COLUMN IF NOT EXISTS "CareRequirements" character varying(2000) NULL;
ALTER TABLE "Residents" ADD COLUMN IF NOT EXISTS "EmergencyContactName" character varying(120) NULL;
ALTER TABLE "Residents" ADD COLUMN IF NOT EXISTS "EmergencyContactNumber" character varying(40) NULL;
ALTER TABLE "Residents" ADD COLUMN IF NOT EXISTS "EmergencyContactRelationship" character varying(80) NULL;
ALTER TABLE "Residents" ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT TRUE;

UPDATE "Residents" SET "IsActive" = TRUE WHERE "IsActive" IS NULL;
