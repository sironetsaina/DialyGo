using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;

        public PatientController(MobileDialysisDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDetailDto>> GetPatientDetails(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            return Ok(new PatientDetailDto
            {
                PatientId = patient.PatientId,
                Name = patient.Name,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address,
                MedicalHistory = patient.MedicalHistory
            });
        }

        [HttpGet("appointments/available/{truckId}")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableAppointments(int truckId)
        {
            var bookedDates = await _context.Appointments
                .Where(a => a.TruckId == truckId && a.Status != "Cancelled")
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            // Example: allow booking for next 7 days, 9am–5pm hourly
            var available = new List<DateTime>();
            for (int day = 0; day < 7; day++)
            {
                var date = DateTime.Today.AddDays(day);
                for (int hour = 9; hour <= 17; hour++)
                {
                    var slot = date.AddHours(hour);
                    if (!bookedDates.Contains(slot))
                        available.Add(slot);
                }
            }

            return Ok(available);
        }

        [HttpPost("appointments")]
        public async Task<ActionResult<AppointmentDto>> BookAppointment([FromBody] AppointmentCreateDto dto)
        {
            var exists = await _context.Appointments.AnyAsync(a =>
                a.TruckId == dto.TruckId &&
                a.AppointmentDate == dto.AppointmentDate &&
                a.Status != "Cancelled");

            if (exists) return Conflict("This time slot is already booked.");

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                TruckId = dto.TruckId,
                AppointmentDate = dto.AppointmentDate,
                Status = "Booked"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                TruckPlate = (await _context.Trucks.FindAsync(dto.TruckId))?.LicensePlate
            });
        }

        [HttpGet("appointments/{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Truck)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            return Ok(new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Notes = appointment.Notes,
                TruckPlate = appointment.Truck?.LicensePlate
            });
        }

        [HttpPut("appointments/{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Cancelled";
            _context.Entry(appointment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpGet("trucks/{truckId}/location")]
        public async Task<ActionResult<TruckLocationDto>> GetTruckLocation(int truckId)
        {
            var truck = await _context.Trucks.FindAsync(truckId);
            if (truck == null) return NotFound();

            return Ok(new TruckLocationDto
            {
                TruckId = truck.TruckId,
                LicensePlate = truck.LicensePlate,
                CurrentLocation = truck.CurrentLocation
            });
        }
    }
}
