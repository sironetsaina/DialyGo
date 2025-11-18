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

        [HttpGet("doctors")]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
        {
            var doctors = await _context.Doctors.ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("doctors/{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

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

        [HttpPut("doctors/{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorCreateDto dto)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            doctor.Name = dto.Name;
            doctor.Email = dto.Email;
            doctor.Specialization = dto.Specialization;
            doctor.PhoneNumber = dto.PhoneNumber;

            _context.Entry(doctor).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("nurses")]
        public async Task<ActionResult<IEnumerable<Nurse>>> GetNurses()
        {
            var nurses = await _context.Nurses.ToListAsync();
            return Ok(nurses);
        }

        [HttpGet("nurses/{id}")]
        public async Task<ActionResult<Nurse>> GetNurse(int id)
        {
            var nurse = await _context.Nurses.FindAsync(id);
            if (nurse == null) return NotFound();
            return Ok(nurse);
        }

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

        [HttpPut("nurses/{id}")]
        public async Task<IActionResult> UpdateNurse(int id, [FromBody] NurseCreateDto dto)
        {
            var nurse = await _context.Nurses.FindAsync(id);
            if (nurse == null) return NotFound();

            nurse.Name = dto.Name;
            nurse.Email = dto.Email;
            nurse.PhoneNumber = dto.PhoneNumber;

            _context.Entry(nurse).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("nurses/{id}")]
        public async Task<IActionResult> DeleteNurse(int id)
        {
            var nurse = await _context.Nurses.FindAsync(id);
            if (nurse == null) return NotFound();

            _context.Nurses.Remove(nurse);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
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

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserCreateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Username = dto.Username;
            user.Password = dto.Password;
            user.RoleId = dto.RoleId;
            user.RelatedId = dto.RelatedId;
            user.IsActive = dto.IsActive;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("patients")]
        public async Task<ActionResult<IEnumerable<object>>> GetPatients()
        {
            var patients = await _context.Patients
                .Select(p => new { p.PatientId, p.Name })
                .ToListAsync();

            return Ok(patients);
        }

        // ================= TRUCK CRUD =================
        [HttpGet("trucks")]
        public async Task<ActionResult<IEnumerable<Truck>>> GetTrucks()
        {
            var trucks = await _context.Trucks.ToListAsync();
            return Ok(trucks);
        }

        [HttpGet("trucks/{id}")]
        public async Task<ActionResult<Truck>> GetTruck(int id)
        {
            var truck = await _context.Trucks.FindAsync(id);
            if (truck == null) return NotFound();
            return Ok(truck);
        }

        [HttpPost("trucks")]
        public async Task<IActionResult> CreateTruck([FromBody] TruckCreateDto dto)
        {
            var truck = new Truck
            {
                LicensePlate = dto.LicensePlate,
                CurrentLocation = dto.CurrentLocation,
                Capacity = dto.Capacity
            };

            _context.Trucks.Add(truck);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTruck), new { id = truck.TruckId }, truck);
        }

        [HttpPut("trucks/{id}")]
        public async Task<IActionResult> UpdateTruck(int id, [FromBody] TruckCreateDto dto)
        {
            var truck = await _context.Trucks.FindAsync(id);
            if (truck == null) return NotFound();

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
            if (truck == null) return NotFound();

            _context.Trucks.Remove(truck);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
