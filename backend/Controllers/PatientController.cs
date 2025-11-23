using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;
        private readonly SmsService _smsService;

        public PatientController(MobileDialysisDbContext context, SmsService smsService)
        {
            _context = context;
            _smsService = smsService;
        }

        // ================================
        // GET PATIENT DETAILS
        // ================================
        [HttpGet("{patientId}")]
        public async Task<ActionResult<PatientDetailDto>> GetPatient(int patientId)
        {
            var patient = await _context.Patients
                .Where(p => p.PatientId == patientId)
                .Select(p => new PatientDetailDto
                {
                    PatientId = p.PatientId,
                    Name = p.Name,
                    Gender = p.Gender,
                    DateOfBirth = p.DateOfBirth,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    Address = p.Address,
                    MedicalHistory = p.MedicalHistory,
                    AppointmentID = p.Appointments.Select(a => new AppointmentDto
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate,
                        Status = a.Status,
                        Notes = a.Notes,
                        TruckPlate = a.Truck != null ? a.Truck.LicensePlate : null,
                        TruckLocation = a.Truck != null ? a.Truck.CurrentLocation : null
                    }).ToList(),
                    TreatmentRecords = p.TreatmentRecords
                })
                .FirstOrDefaultAsync();

            if (patient == null) return NotFound("❌ Patient not found.");
            return Ok(patient);
        }

        // ================================
        // GET LIST OF TRUCKS
        // ================================
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

        // ================================
        // GET AVAILABLE SLOTS
        // ================================
        [HttpGet("appointments/available/{truckId}/{date}")]
        public async Task<ActionResult<IEnumerable<string>>> GetAvailableSlots(int truckId, string date)
        {
            var truck = await _context.Trucks.FindAsync(truckId);
            if (truck == null) return NotFound("Truck not found");

            if (!DateTime.TryParse(date, out var selectedDate))
                return BadRequest("Invalid date format. Use yyyy-MM-dd");

            var slots = new List<string>();
            var start = new TimeSpan(6, 0, 0);
            var end = new TimeSpan(20, 0, 0);

            for (var t = start; t < end; t = t.Add(TimeSpan.FromHours(4)))
            {
                var slotStart = t;
                var slotEnd = t.Add(TimeSpan.FromHours(4));

                var count = await _context.Appointments.CountAsync(a =>
                    a.TruckId == truckId &&
                    a.Status != "Cancelled" &&
                    a.AppointmentDate.Date == selectedDate.Date &&
                    a.AppointmentDate.TimeOfDay >= slotStart &&
                    a.AppointmentDate.TimeOfDay < slotEnd
                );

                if (count < truck.Capacity)
                    slots.Add($"{slotStart:hh\\:mm}-{slotEnd:hh\\:mm}");
            }

            return Ok(slots);
        }

        // ================================
        // BOOK APPOINTMENT
        // ================================
        [HttpPost("appointments")]
        public async Task<ActionResult> BookAppointment([FromBody] AppointmentCreateDto dto)
        {
            var patient = await _context.Patients.FindAsync(dto.PatientId);
            var truck = await _context.Trucks.FindAsync(dto.TruckId);

            if (patient == null || truck == null)
                return BadRequest("Invalid patient or truck.");

            var dateOnly = dto.AppointmentDate.Date;
            var exists = await _context.Appointments
                .AnyAsync(a => a.PatientId == dto.PatientId && a.AppointmentDate.Date == dateOnly && a.Status != "Cancelled");

            if (exists)
                return Conflict("Patient already has an appointment that day.");

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                TruckId = dto.TruckId,
                AppointmentDate = dto.AppointmentDate,
                Status = "Scheduled"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // ---------------- Create Notification ----------------
            var notifMessage = $"Your appointment for {dto.AppointmentDate:dddd, MMM dd yyyy HH:mm} on truck {truck.LicensePlate} has been booked successfully.";
            var notification = new Smsnotification
            {
                PatientId = patient.PatientId,
                Message = notifMessage,
                SentAt = DateTime.UtcNow,
                SentBy = "System",
                SenderId = 0,
                SenderRole = "Sys"
            };
            _context.Smsnotifications.Add(notification);
            await _context.SaveChangesAsync();

            try
            {
                await _smsService.SendSms(patient.PhoneNumber, notifMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMS sending failed: {ex.Message}");
            }

            return Ok(new { Message = notifMessage });
        }

        // ================================
        // CANCEL APPOINTMENT
        // ================================
        [HttpPost("appointments/cancel/{appointmentId}")]
        public async Task<ActionResult> CancelAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound("Appointment not found.");
            if (appointment.Status == "Cancelled") return BadRequest("Appointment already cancelled.");

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            var patient = await _context.Patients.FindAsync(appointment.PatientId);
            var truck = await _context.Trucks.FindAsync(appointment.TruckId);

            if (patient != null && truck != null)
            {
                var notifMessage = $"Hello {patient.Name}, your appointment on {appointment.AppointmentDate:dddd, MMM dd yyyy HH:mm} has been cancelled.";
                var notification = new Smsnotification
                {
                    PatientId = patient.PatientId,
                    Message = notifMessage,
                    SentAt = DateTime.UtcNow,
                    SentBy = "System",
                    SenderId = 0,
                    SenderRole = "Sys"
                };
                _context.Smsnotifications.Add(notification);
                await _context.SaveChangesAsync();

                try
                {
                    await _smsService.SendSms(patient.PhoneNumber, notifMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SMS sending failed: {ex.Message}");
                }
            }

            return Ok(new { Message = "Appointment cancelled successfully!" });
        }

        // ================================
        // GET PATIENT NOTIFICATIONS
        // ================================
        [HttpGet("notifications/{patientId}")]
        public async Task<ActionResult<IEnumerable<Smsnotification>>> GetNotifications(int patientId)
        {
            var exists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!exists) return NotFound("❌ Patient not found.");

            var notifications = await _context.Smsnotifications
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
