# HomeCarePH — Phase 7 launch runbook

## Delivered

- Dashboard summaries for upcoming maintenance, property overview, appliance warranty alerts, and the last-30-day expense summary.
- Responsive dashboard layouts for tablet and mobile widths, with keyboard-visible links and semantic status regions.
- `GET /diagnostics/health` returns JSON and HTTP 200 only when the application can reach PostgreSQL; it returns HTTP 503 when degraded.
- Global antiforgery validation for unsafe MVC requests, secure HTTP-only auth cookies, forwarded-header support, and baseline security response headers.
- Reproducible multi-stage `Dockerfile` listening on `PORT`-compatible port 3000, plus `.dockerignore`.

## Security verification before production

1. Set `ConnectionStrings__DefaultConnection` through the deployment secret store; do not commit credentials.
2. Run the app behind HTTPS and verify `/diagnostics/health` is the only unauthenticated operational endpoint exposed.
3. Verify login, logout, create, edit, delete, invitation, repair, quotation, booking, and maintenance forms reject requests without an antiforgery token.
4. Confirm the response headers include `Strict-Transport-Security` (outside Development), `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, and `Permissions-Policy`.
5. Confirm a locked account cannot retry until the configured 15-minute lockout expires.

## Backup and restore verification

Use a managed PostgreSQL snapshot or `pg_dump` before release. Restore into a disposable database, set `ConnectionStrings__DefaultConnection` to the restored database, start the container, wait for migrations, and verify `/diagnostics/health` plus dashboard counts. Record the snapshot identifier, restore timestamp, migration output, and a sample property/appliance/maintenance record. Never run restore tests against the production database.

## Production deployment

Build with `docker build -t homecareph:phase7 .`, run with `-e ConnectionStrings__DefaultConnection=... -p 3000:3000`, and configure the platform health check to `/diagnostics/health`. The container performs EF Core migrations on startup; use a release window and a database backup before applying migrations.

The provided sandbox does not include the .NET SDK, so local compile and integration execution could not be run here. Run `dotnet test`/`dotnet build` or the container build in CI before publishing, then complete the security and restore verification above with a real PostgreSQL instance.
