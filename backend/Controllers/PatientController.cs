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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public PatientController(
            MobileDialysisDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
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

            if (patient == null) 
                return NotFound("❌ Patient not found.");

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
        // GET AVAILABLE SLOTS FOR TRUCK + DATE
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

            // Check if patient already has an appointment that day
            var dateOnly = dto.AppointmentDate.Date;
            var existingAppointment = await _context.Appointments
                .AnyAsync(a => a.PatientId == dto.PatientId && a.AppointmentDate.Date == dateOnly && a.Status != "Cancelled");

            if (existingAppointment)
                return Conflict("Patient already has an appointment that day.");

            // Determine slot start/end for the chosen appointment
            var appointmentTime = dto.AppointmentDate.TimeOfDay;
            var slotStart = new TimeSpan(appointmentTime.Hours / 4 * 4, 0, 0);
            var slotEnd = slotStart.Add(TimeSpan.FromHours(4));

            // Check truck capacity
            var bookedCount = await _context.Appointments.CountAsync(a =>
                a.TruckId == dto.TruckId &&
                a.Status != "Cancelled" &&
                a.AppointmentDate.TimeOfDay >= slotStart &&
                a.AppointmentDate.TimeOfDay < slotEnd);

            if (bookedCount >= truck.Capacity)
                return Conflict("Slot is fully booked.");

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                TruckId = dto.TruckId,
                AppointmentDate = dto.AppointmentDate,
                Status = "Scheduled"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // ✅ Send SMS notification
            try
            {
                var client = _httpClientFactory.CreateClient();
                var smsController = new SmsController(client, _config);
                await smsController.SendSms(new SmsController.SmsRequest
                {
                    PhoneNumber = patient.PhoneNumber,
                    Message = $"Hello {patient.Name}, your appointment is confirmed for {dto.AppointmentDate:dddd, MMM dd yyyy HH:mm} on truck {truck.LicensePlate}."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMS error: {ex.Message}");
            }

            return Ok(new { Message = "Appointment booked successfully!" });
        }

        // ================================
        // GET APPOINTMENTS FOR PATIENT
        // ================================
        [HttpGet("appointments/{patientId}")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(int patientId)
        {
            var exists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!exists) return NotFound("❌ Patient not found.");

            var appointments = await _context.Appointments
                .Include(a => a.Truck)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Notes = a.Notes,
                    TruckPlate = a.Truck != null ? a.Truck.LicensePlate : null,
                    TruckLocation = a.Truck != null ? a.Truck.CurrentLocation : null
                })
                .ToListAsync();

            return Ok(appointments);
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

            // Send cancellation SMS
            try
            {
                var patient = await _context.Patients.FindAsync(appointment.PatientId);
                var truck = await _context.Trucks.FindAsync(appointment.TruckId);

                if (patient != null && truck != null)
                {
                    var client = _httpClientFactory.CreateClient();
                    var smsController = new SmsController(client, _config);
                    await smsController.SendSms(new SmsController.SmsRequest
                    {
                        PhoneNumber = patient.PhoneNumber,
                        Message = $"Hello {patient.Name}, your appointment on {appointment.AppointmentDate:dddd, MMM dd yyyy HH:mm} has been cancelled."
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMS error: {ex.Message}");
            }

            return Ok(new { Message = "Appointment cancelled successfully!" });
        }
    }
}
