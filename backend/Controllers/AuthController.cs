using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;

        public AuthController(MobileDialysisDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO login)
        {
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
                return BadRequest(new { message = "Username and password are required." });

            // Find the user by Username
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == login.Username);

            if (user == null)
                return Unauthorized(new { message = "Username not found." });

            // Check password (plain text for now, replace with hash comparison if needed)
            if (user.Password != login.Password)
                return Unauthorized(new { message = "Incorrect password." });

            // If the user is a patient, get their full name
            string? patientName = null;
            if (user.RoleId == 4) // assuming 4 = Patient role
            {
                var patient = await _context.Patients.FindAsync(user.RelatedId);
                patientName = patient?.Name;
            }

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                role = user.Role?.RoleName ?? "Unknown",
                relatedId = user.RelatedId,
                fullName = patientName
            });
        }
    }

    public class LoginRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
