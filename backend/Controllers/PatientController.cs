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

                    AppointmentID = p.Appointments
                        .OrderBy(a => a.AppointmentId)   // numbering
                        .Select(a => new AppointmentDto
                        {
                            AppointmentId = a.AppointmentId,
                            AppointmentNumber = a.AppointmentId,   // <-- numbered like 1,2,3,4...
                            AppointmentDate = a.AppointmentDate,
                            Status = a.Status,
                            Notes = a.Notes,
                            TruckId = a.TruckId,
                            TruckPlate = a.Truck != null ? a.Truck.LicensePlate : null,
                            TruckLocation = a.Truck != null ? a.Truck.CurrentLocation : null
                        }).ToList(),

                    TreatmentRecords = p.TreatmentRecords
                })
                .FirstOrDefaultAsync();

            if (patient == null)
                return NotFound("❌ Patient not found.");

            return Ok(patient);
        }

        
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

        
        [HttpPost("appointments")]
        public async Task<ActionResult> BookAppointment([FromBody] AppointmentCreateDto dto)
        {
            var patient = await _context.Patients.FindAsync(dto.PatientId);
            var truck = await _context.Trucks.FindAsync(dto.TruckId);

            if (patient == null || truck == null)
                return BadRequest("Invalid patient or truck.");

            var dateOnly = dto.AppointmentDate.Date;

            var exists = await _context.Appointments.AnyAsync(a =>
                a.PatientId == dto.PatientId &&
                a.AppointmentDate.Date == dateOnly &&
                a.Status != "Cancelled");

            if (exists)
                return Conflict("Patient already has an appointment that day.");

            var newAppointment = new Appointment
            {
                PatientId = dto.PatientId,
                TruckId = dto.TruckId,
                AppointmentDate = dto.AppointmentDate,
                Status = "Scheduled"
            };

            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            var message =
                $"Your appointment for {dto.AppointmentDate:dddd, MMM dd yyyy HH:mm} on truck {truck.LicensePlate} has been booked.";

            _context.Smsnotifications.Add(new Smsnotification
            {
                PatientId = patient.PatientId,
                Message = message,
                SentAt = DateTime.UtcNow,
                SentBy = "System",
                SenderId = 0,
                SenderRole = "Sys"
            });

            await _context.SaveChangesAsync();

            try
            {
                await _smsService.SendSms(patient.PhoneNumber, message);
            }
            catch { }

            return Ok(new { Message = message });
        }

      
        [HttpPost("appointments/cancel/{appointmentId}")]
        public async Task<ActionResult> CancelAppointment(int appointmentId)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt == null) return NotFound("Appointment not found.");

            if (appt.Status == "Completed")
                return BadRequest("Completed appointments cannot be cancelled.");

            if (appt.Status == "Missed") 
                return BadRequest("Missed appointments cannot be cancelled.");

            if (appt.Status == "Cancelled")
                return BadRequest("This appointment is already cancelled.");

            appt.Status = "Cancelled";
            await _context.SaveChangesAsync();

            var patient = await _context.Patients.FindAsync(appt.PatientId);

            if (patient != null)
            {
                string msg =
                    $"Hello {patient.Name}, your appointment on {appt.AppointmentDate:dddd, MMM dd yyyy HH:mm} has been cancelled.";

                _context.Smsnotifications.Add(new Smsnotification
                {
                    PatientId = patient.PatientId,
                    Message = msg,
                    SentAt = DateTime.UtcNow,
                    SentBy = "System",
                    SenderId = 0,
                    SenderRole = "Sys"
                });

                await _context.SaveChangesAsync();

                try { await _smsService.SendSms(patient.PhoneNumber, msg); }
                catch { }
            }

            return Ok(new { Message = "Appointment cancelled." });
        }

       
        [HttpPost("appointments/check-missed")]
        public async Task<ActionResult> CheckMissedAppointments()
        {
            var now = DateTime.UtcNow;

            var missed = await _context.Appointments
                .Where(a => a.Status == "Scheduled" && a.AppointmentDate < now)
                .ToListAsync();

            if (!missed.Any())
                return Ok(new { Message = "No missed appointments." });

            foreach (var appt in missed)
            {
                appt.Status = "Missed";

                var patient = await _context.Patients.FindAsync(appt.PatientId);
                if (patient == null) continue;

                string text =
                    $"You missed your appointment on {appt.AppointmentDate:dddd, MMM dd yyyy HH:mm}. Please rebook.";

                _context.Smsnotifications.Add(new Smsnotification
                {
                    PatientId = patient.PatientId,
                    Message = text,
                    SentAt = DateTime.UtcNow,
                    SentBy = "System",
                    SenderId = 0,
                    SenderRole = "Sys"
                });

                try { await _smsService.SendSms(patient.PhoneNumber, text); }
                catch { }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Missed appointments processed." });
        }

     
        [HttpGet("notifications/stream/{patientId}")]
        public async Task GetNotificationStream(int patientId)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");

            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                var messages = await _context.Smsnotifications
                    .Where(n => n.PatientId == patientId)
                    .OrderByDescending(n => n.SentAt)
                    .Select(n => n.Message)
                    .ToListAsync();

                string json = System.Text.Json.JsonSerializer.Serialize(messages);

                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();

                await Task.Delay(5000);
            }
        }

        
        [HttpGet("notifications/{patientId}")]
        public async Task<ActionResult<IEnumerable<Smsnotification>>> GetNotifications(int patientId)
        {
            var exists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!exists) return NotFound("Patient not found.");

            var messages = await _context.Smsnotifications
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
