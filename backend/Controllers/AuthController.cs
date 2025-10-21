using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

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

        // ✅ Test database connection
        [HttpGet("test-db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                var sampleUsers = await _context.Users
                    .Select(u => new { u.UserId, u.Username, u.RoleId })
                    .Take(5)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Database connection successful ✅",
                    totalUsers = userCount,
                    sample = sampleUsers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Database connection failed ❌",
                    error = ex.Message
                });
            }
        }

        // ✅ POST /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO login)
        {
            if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
                return BadRequest(new { message = "Username and password are required." });

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == login.Username);

            if (user == null)
                return Unauthorized(new { message = "Username not found." });

            if (user.Password != login.Password)
                return Unauthorized(new { message = "Incorrect password." });

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                role = user.Role?.RoleName ?? "Unknown"
            });
        }
    }

    public class LoginRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
