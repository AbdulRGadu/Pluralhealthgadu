using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Data;

namespace pluralhealth_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> GetAppointmentTypes()
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var appointmentTypes = await _context.AppointmentTypes
                .Where(at => at.FacilityId == facilityId)
                .Select(at => new { at.Id, at.Name, at.DefaultDurationMinutes })
                .OrderBy(at => at.Name)
                .ToListAsync();

            return Ok(appointmentTypes);
        }
    }
}

