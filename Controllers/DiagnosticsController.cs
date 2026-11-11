using HomeCarePH.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Controllers;

[Route("diagnostics")]
public class DiagnosticsController(ApplicationDbContext db, ILogger<DiagnosticsController> logger) : Controller
{
    [HttpGet("health")]
    [Produces("application/json")]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        try
        {
            var connected = await db.Database.CanConnectAsync(cancellationToken);
            return connected
                ? Ok(new { status = "ok", database = "connected", utc = DateTime.UtcNow })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "degraded", database = "unavailable" });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Production health check failed.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "degraded", database = "unavailable" });
        }
    }

    [HttpGet("database")]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
    {
        var result = await Health(cancellationToken);
        return result is ObjectResult { StatusCode: StatusCodes.Status200OK }
            ? Ok("HomeCare PH database connection successful.")
            : StatusCode(StatusCodes.Status503ServiceUnavailable, "Database connection failed.");
    }
}
