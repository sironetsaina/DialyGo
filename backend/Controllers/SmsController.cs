using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;
        private readonly SmsService _smsService;

        public SmsController(MobileDialysisDbContext context, SmsService smsService)
        {
            _context = context;
            _smsService = smsService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSms([FromBody] SmsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Phone number and message are required.");

            var success = await _smsService.SendSms(request.PhoneNumber, request.Message);

            if (!success) return StatusCode(500, "SMS sending failed.");

            // Save to DB
            var notification = new Smsnotification
            {
                PatientId = request.PatientId,
                Message = request.Message,
                SentAt = DateTime.UtcNow,
                SentBy = request.SentBy ?? "System",
                SenderId = request.SenderId,
                SenderRole = request.SenderRole
            };
            _context.Smsnotifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "SMS sent and saved successfully." });
        }

        public class SmsRequest
        {
            public string PhoneNumber { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public int PatientId { get; set; }
            public string? SentBy { get; set; }
            public int? SenderId { get; set; }
            public string? SenderRole { get; set; }
        }
    }
}
