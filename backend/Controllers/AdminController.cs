using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;

        public AdminController(MobileDialysisDbContext context)
        {
            _context = context;
        }

        // DOCTOR CRUD 
        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorCreateDto dto)
        {
            var doctor = new Doctor
            {
                Name = dto.Name,
                Email = dto.Email,
                Specialization = dto.Specialization,
                PhoneNumber = dto.PhoneNumber
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, doctor);
        }

        [HttpGet("doctors/{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        // ================= NURSE CRUD =================
        [HttpPost("nurses")]
        public async Task<IActionResult> CreateNurse([FromBody] NurseCreateDto dto)
        {
            var nurse = new Nurse
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            _context.Nurses.Add(nurse);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNurse), new { id = nurse.NurseId }, nurse);
        }

        [HttpGet("nurses/{id}")]
        public async Task<IActionResult> GetNurse(int id)
        {
            var nurse = await _context.Nurses.FindAsync(id);
            if (nurse == null) return NotFound();
            return Ok(nurse);
        }

        //USERS Crud
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password, // TODO: Hash before saving in production!
                RoleId = dto.RoleId,
                RelatedId = dto.RelatedId,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        //=
        [HttpGet("trucks")]
        public async Task<ActionResult<IEnumerable<TruckDto>>> GetTrucks()
        {
            var trucks = await _context.Trucks
                .Select(t => new TruckDto
                {
                    TruckId = t.TruckId,
                    LicensePlate = t.LicensePlate,
                    CurrentLocation = t.CurrentLocation,
                    Capacity = t.Capacity
                })
                .ToListAsync();

            return Ok(trucks);
        }

        [HttpGet("trucks/{id}")]
        public async Task<ActionResult<TruckDto>> GetTruck(int id)
        {
            var truck = await _context.Trucks.FindAsync(id);

            if (truck == null)
                return NotFound();

            return Ok(new TruckDto
            {
                TruckId = truck.TruckId,
                LicensePlate = truck.LicensePlate,
                CurrentLocation = truck.CurrentLocation,
                Capacity = truck.Capacity
            });
        }

        [HttpPost("trucks")]
        public async Task<ActionResult<TruckDto>> CreateTruck([FromBody] TruckCreateDto dto)
        {
            var truck = new Truck
            {
                LicensePlate = dto.LicensePlate,
                CurrentLocation = dto.CurrentLocation,
                Capacity = dto.Capacity
            };

            _context.Trucks.Add(truck);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTruck), new { id = truck.TruckId }, new TruckDto
            {
                TruckId = truck.TruckId,
                LicensePlate = truck.LicensePlate,
                CurrentLocation = truck.CurrentLocation,
                Capacity = truck.Capacity
            });
        }

        [HttpPut("trucks/{id}")]
        public async Task<IActionResult> UpdateTruck(int id, [FromBody] TruckCreateDto dto)
        {
            var truck = await _context.Trucks.FindAsync(id);
            if (truck == null)
                return NotFound();

            truck.LicensePlate = dto.LicensePlate;
            truck.CurrentLocation = dto.CurrentLocation;
            truck.Capacity = dto.Capacity;

            _context.Entry(truck).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("trucks/{id}")]
        public async Task<IActionResult> DeleteTruck(int id)
        {
            var truck = await _context.Trucks.FindAsync(id);
            if (truck == null)
                return NotFound();

            _context.Trucks.Remove(truck);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // TRUCK STAFF ASSIGNMENTS
        [HttpGet("truck-staff")]
        public async Task<ActionResult<IEnumerable<TruckStaffAssignmentDto>>> GetTruckStaffAssignments()
        {
            var assignments = await _context.TruckStaffAssignments
                .Select(a => new TruckStaffAssignmentDto
                {
                    AssignmentId = a.AssignmentId,
                    TruckId = a.TruckId,
                    StaffId = a.StaffId,
                    Role = a.Role,
                    DateAssigned = a.DateAssigned
                })
                .ToListAsync();

            return Ok(assignments);
        }

        [HttpGet("truck-staff/{id}")]
        public async Task<ActionResult<TruckStaffAssignmentDto>> GetTruckStaffAssignment(int id)
        {
            var assignment = await _context.TruckStaffAssignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            return Ok(new TruckStaffAssignmentDto
            {
                AssignmentId = assignment.AssignmentId,
                TruckId = assignment.TruckId,
                StaffId = assignment.StaffId,
                Role = assignment.Role,
                DateAssigned = assignment.DateAssigned
            });
        }

        [HttpPost("truck-staff")]
        public async Task<ActionResult<TruckStaffAssignmentDto>> AssignStaffToTruck([FromBody] TruckStaffAssignmentCreateDto dto)
        {
            var assignment = new TruckStaffAssignment
            {
                TruckId = dto.TruckId,
                StaffId = dto.StaffId,
                Role = dto.Role,
                DateAssigned = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _context.TruckStaffAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTruckStaffAssignment), new { id = assignment.AssignmentId }, new TruckStaffAssignmentDto
            {
                AssignmentId = assignment.AssignmentId,
                TruckId = assignment.TruckId,
                StaffId = assignment.StaffId,
                Role = assignment.Role,
                DateAssigned = assignment.DateAssigned
            });
        }

        [HttpDelete("truck-staff/{id}")]
        public async Task<IActionResult> RemoveStaffFromTruck(int id)
        {
            var assignment = await _context.TruckStaffAssignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            _context.TruckStaffAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
