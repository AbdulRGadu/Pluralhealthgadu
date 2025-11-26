using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pluralhealth_API.Data;

namespace pluralhealth_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClinicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> GetClinics()
        {
            var facilityId = (int)(HttpContext.Items["FacilityId"] ?? 1);

            var clinics = await _context.Clinics
                .Where(c => c.FacilityId == facilityId)
                .Select(c => new { c.Id, c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(clinics);
        }
    }
}

