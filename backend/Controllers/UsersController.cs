 using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;

        public UsersController(MobileDialysisDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    RoleId = u.RoleId,  // No null check since it's non-nullable
                    RoleName = u.Role != null ? u.Role.RoleName : "",
                    RelatedId = u.RelatedId,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.UserId == id)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName : "",
                    RelatedId = u.RelatedId,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(UserCreateDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password, 
                RoleId = dto.RoleId,
                RelatedId = dto.RelatedId,
                IsActive = dto.IsActive
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var createdDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleId = user.RoleId,
                RoleName = (await _context.UserRoles.FindAsync(user.RoleId))?.RoleName ?? "",
                RelatedId = user.RelatedId,
                IsActive = user.IsActive
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, createdDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserCreateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = dto.Username;
            user.Password = dto.Password; 
            user.RoleId = dto.RoleId;
            user.RelatedId = dto.RelatedId;
            user.IsActive = dto.IsActive;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.UserId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
