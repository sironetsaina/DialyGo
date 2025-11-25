using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsNotificationController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;

        public SmsNotificationController(MobileDialysisDbContext context)
        {
            _context = context;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientNotifications(int patientId)
        {
            var notifications = await _context.Smsnotifications
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.SentAt)   
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _context.Smsnotifications
                .OrderByDescending(n => n.SentAt) 
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
