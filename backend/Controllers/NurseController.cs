using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NurseController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public NurseController(
            MobileDialysisDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        // ---------------- 1️⃣ REGISTER NEW PATIENT ----------------
        [HttpPost("patients")]
        public async Task<ActionResult<PatientDto>> RegisterPatient([FromBody] PatientCreateDto dto)
        {
            var patient = new Patient
            {
                Name = dto.Name,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                MedicalHistory = dto.MedicalHistory
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Mark patient as seen today
            var tempAppointment = new Appointment
            {
                PatientId = patient.PatientId,
                Status = "Completed",
                AppointmentDate = DateTime.UtcNow,
                Notes = "New patient registered counts as seen"
            };
            _context.Appointments.Add(tempAppointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId }, new PatientDto
            {
                PatientId = patient.PatientId,
                Name = patient.Name,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address
            });
        }

        // ---------------- 2️⃣ GET SINGLE PATIENT ----------------
        [HttpGet("patients/{id}")]
        public async Task<ActionResult<PatientDto>> GetPatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            return Ok(new PatientDto
            {
                PatientId = patient.PatientId,
                Name = patient.Name,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address
            });
        }

        // ---------------- 3️⃣ UPDATE PATIENT DETAILS ----------------
        [HttpPut("patients/{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpdateDto dto)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            patient.Name = dto.Name;
            patient.Gender = dto.Gender;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.PhoneNumber = dto.PhoneNumber;
            patient.Email = dto.Email;
            patient.Address = dto.Address;
            patient.MedicalHistory = dto.MedicalHistory;

            await _context.SaveChangesAsync();

            // Mark patient as seen today
            var tempAppointment = new Appointment
            {
                PatientId = patient.PatientId,
                Status = "Completed",
                AppointmentDate = DateTime.UtcNow,
                Notes = "Patient updated counts as seen"
            };
            _context.Appointments.Add(tempAppointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------- 4️⃣ GET TREATMENT HISTORY ----------------
        [HttpGet("patients/{id}/treatment-summary")]
        public async Task<ActionResult<IEnumerable<TreatmentEntryDto>>> GetTreatmentSummary(int id)
        {
            var treatments = await _context.TreatmentRecords
                .Where(t => t.PatientId == id)
                .Select(tr => new TreatmentEntryDto
                {
                    Diagnosis = tr.Diagnosis,
                    TreatmentDetails = tr.TreatmentDetails,
                    TreatmentDate = tr.TreatmentDate ?? DateTime.MinValue
                })
                .ToListAsync();

            if (treatments == null || treatments.Count == 0)
                return NotFound("No treatment records found for this patient.");

            return Ok(treatments);
        }

        // ---------------- 5️⃣ GET NURSE BY ID ----------------
        [HttpGet("{id}")]
        public async Task<ActionResult<NurseCreateDto>> GetNurseById(int id)
        {
            var nurse = await _context.Nurses.FindAsync(id);
            if (nurse == null) return NotFound();

            return Ok(new NurseCreateDto
            {
                NurseId = nurse.NurseId,
                Name = nurse.Name,
                Email = nurse.Email,
                PhoneNumber = nurse.PhoneNumber
            });
        }

        // ---------------- 6️⃣ GET ALL PATIENTS ----------------
        [HttpGet("patients")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetAllPatients()
        {
            var patients = await _context.Patients
                .Select(p => new PatientDto
                {
                    PatientId = p.PatientId,
                    Name = p.Name,
                    Gender = p.Gender,
                    DateOfBirth = p.DateOfBirth,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    Address = p.Address
                })
                .ToListAsync();

            return Ok(patients);
        }

        // ---------------- 7️⃣ GET PATIENT APPOINTMENTS ----------------
        [HttpGet("patients/{patientId}/appointments")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetPatientAppointments(int patientId)
        {
            var exists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!exists) return NotFound("Patient not found.");

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

        // ---------------- 8️⃣ GET PATIENTS SEEN TODAY ----------------
        [HttpGet("patients/seen-today")]
        public async Task<IActionResult> GetPatientsSeenToday()
        {
            var today = DateTime.UtcNow.Date;

            var patients = await _context.Appointments
                .Where(a => a.Status == "Completed" &&
                            a.AppointmentDate >= today &&
                            a.AppointmentDate < today.AddDays(1))
                .Include(a => a.Patient)
                .Select(a => new
                {
                    patientId = a.Patient.PatientId,
                    name = a.Patient.Name,
                    phoneNumber = a.Patient.PhoneNumber
                })
                .Distinct()
                .OrderBy(p => p.name)
                .ToListAsync();

            return Ok(patients);
        }
        [HttpPut("patients/{patientId}/update")]
public async Task<IActionResult> UpdatePatientDetails(int patientId, [FromBody] PatientUpdateDto dto)
{
    var patient = await _context.Patients.FindAsync(patientId);
    if (patient == null) return NotFound("Patient not found.");

    // Update patient basic details
    patient.Name = dto.Name ?? patient.Name;
    patient.Gender = dto.Gender ?? patient.Gender;
    patient.DateOfBirth = dto.DateOfBirth ?? patient.DateOfBirth;
    patient.PhoneNumber = dto.PhoneNumber ?? patient.PhoneNumber;
    patient.Email = dto.Email ?? patient.Email;
    patient.Address = dto.Address ?? patient.Address;

    // Update medical history
    if (!string.IsNullOrWhiteSpace(dto.MedicalHistory))
    {
        patient.MedicalHistory = dto.MedicalHistory;

        // Add a "medical history record" as a treatment record for tracking
        var latestAppointment = await _context.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .FirstOrDefaultAsync();

        if (latestAppointment != null)
        {
            var treatment = new TreatmentRecord
            {
                PatientId = patientId,
                AppointmentId = latestAppointment.AppointmentId,
                Diagnosis = dto.MedicalHistory,
                TreatmentDetails = "Updated medical history",
                TreatmentDate = DateTime.UtcNow
            };
            _context.TreatmentRecords.Add(treatment);
        }
    }

    _context.Entry(patient).State = EntityState.Modified;
    await _context.SaveChangesAsync();

    return Ok("Patient details and medical history updated successfully.");
}


        // ---------------- 9️⃣ COMPLETE APPOINTMENT ----------------
        [HttpPut("appointments/{appointmentId}/complete")]
        public async Task<IActionResult> CompleteAppointment(
            int appointmentId,
            [FromBody] AppointmentUpdateDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound("Appointment not found.");

            var patient = await _context.Patients.FindAsync(appointment.PatientId);
            if (patient == null) return NotFound("Patient not found.");

            if (string.IsNullOrWhiteSpace(dto.Notes))
                return BadRequest("Notes are required to complete the appointment.");

            appointment.Status = "Completed";
            appointment.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            // Format phone number to international format
            string phoneNumber = patient.PhoneNumber;
            if (!phoneNumber.StartsWith("+"))
                phoneNumber = "+254" + patient.PhoneNumber.TrimStart('0');

            string message = $"Hello {patient.Name}, your dialysis appointment on {appointment.AppointmentDate:dddd, MMM dd yyyy HH:mm} has been completed. Nurse notes: {dto.Notes}";

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_config["Infobip:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Authorization", $"App {_config["Infobip:ApiKey"]}");

                var smsPayload = new
                {
                    PhoneNumber = phoneNumber,
                    Message = message,
                    PatientId = patient.PatientId,
                    SentBy = "Nurse",
                    SenderId = dto.NurseId,
                    SenderRole = "Nurse"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(smsPayload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/Sms/send", content);
                var respBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Save notification in DB
                    var notification = new Smsnotification
                    {
                        PatientId = patient.PatientId,
                        Message = message,
                        SentAt = DateTime.UtcNow,
                        SentBy = "Nurse",
                        SenderId = dto.NurseId,
                        SenderRole = "Nurse"
                    };
                    _context.Smsnotifications.Add(notification);
                    await _context.SaveChangesAsync();

                    return Ok(new { Message = "Appointment completed and SMS sent successfully." });
                }
                else
                {
                    return Ok(new { Message = "Appointment completed, but SMS failed.", Details = respBody });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { Message = "Appointment completed, but SMS failed.", Details = ex.Message });
            }
        }
    }
}
