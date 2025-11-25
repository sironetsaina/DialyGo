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

    var username = login.Username.Trim();
    var password = login.Password.Trim();

    var user = await _context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Username == username);

    if (user == null)
        return Unauthorized(new { message = "Username not found." });

    if (user.IsActive != true)
        return Unauthorized(new { message = "Account is inactive. Contact admin." });

    if (user.Password.Trim() != password)
        return Unauthorized(new { message = "Incorrect password." });

    string? patientName = null;

    if (user.Role?.RoleName == "Patient")
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


    public class LoginRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
}