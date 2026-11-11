using HomeCarePH.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Controllers
{
    [Route("diagnostics")]
    public class DiagnosticsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(
            ApplicationDbContext db,
            ILogger<DiagnosticsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("database")]
        public async Task<IActionResult> Database()
        {
            try
            {
                var connected = await _db.Database.CanConnectAsync();

                if (!connected)
                {
                    return StatusCode(
                        503,
                        "Database connection failed.");
                }

                return Ok(
                    "HomeCare PH database connection successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Database connection test failed.");

                return StatusCode(
                    503,
                    "Database connection failed.");
            }
        }
    }
}