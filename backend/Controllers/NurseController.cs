using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NurseController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;

        public NurseController(MobileDialysisDbContext context)
        {
            _context = context;
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

            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                Status = "Completed",
                AppointmentDate = DateTime.UtcNow,
                Notes = "New patient registered counts as seen"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var treatment = new TreatmentRecord
            {
                PatientId = patient.PatientId,
                AppointmentId = appointment.AppointmentId,
                Diagnosis = "Seen today (new patient)",
                TreatmentDetails = "Auto-generated entry",
                TreatmentDate = DateTime.UtcNow
            };
            _context.TreatmentRecords.Add(treatment);
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

            patient.Name = dto.Name ?? patient.Name;
            patient.Gender = dto.Gender ?? patient.Gender;
            patient.DateOfBirth = dto.DateOfBirth ?? patient.DateOfBirth;
            patient.PhoneNumber = dto.PhoneNumber ?? patient.PhoneNumber;
            patient.Email = dto.Email ?? patient.Email;
            patient.Address = dto.Address ?? patient.Address;

            if (!string.IsNullOrWhiteSpace(dto.MedicalHistory))
            {
                patient.MedicalHistory = dto.MedicalHistory;

                var latestAppointment = await _context.Appointments
                    .Where(a => a.PatientId == id)
                    .OrderByDescending(a => a.AppointmentDate)
                    .FirstOrDefaultAsync();

               if (latestAppointment != null)
{
    // Mark appointment as seen today
    latestAppointment.Status = "Completed";
    latestAppointment.AppointmentDate = DateTime.UtcNow;  // IMPORTANT FIX
    latestAppointment.Notes = "Medical history updated - counted as seen";

    _context.TreatmentRecords.Add(new TreatmentRecord
    {
        PatientId = id,
        AppointmentId = latestAppointment.AppointmentId,
        Diagnosis = dto.MedicalHistory,
        TreatmentDetails = "Updated medical history",
        TreatmentDate = DateTime.UtcNow
    });

    _context.Entry(latestAppointment).State = EntityState.Modified;
}

            }

            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Patient details and medical history updated successfully.");
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

            if (treatments.Count == 0) return NotFound("No treatment records found for this patient.");

            return Ok(treatments);
        }

        // ---------------- 5️⃣ GET ALL PATIENTS ----------------
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

        // ---------------- 6️⃣ GET PATIENTS SEEN TODAY ----------------
        [HttpGet("patients/seen-today")]
        public async Task<IActionResult> GetPatientsSeenToday()
        {
            var today = DateTime.UtcNow.Date;
            return await GetPatientsSeenByDate(today);
        }

        // ---------------- 7️⃣ GET PATIENTS SEEN ON SPECIFIC DATE ----------------
        [HttpGet("patients/seen-on")]
        public async Task<IActionResult> GetPatientsSeenOnDate([FromQuery] DateTime date)
        {
            return await GetPatientsSeenByDate(date.Date);
        }

        private async Task<IActionResult> GetPatientsSeenByDate(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            var appointments = await _context.Appointments
                .Where(a => a.Status == "Completed" &&
                            a.AppointmentDate >= start &&
                            a.AppointmentDate < end)
                .Include(a => a.Patient)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                var exists = await _context.TreatmentRecords
                    .AnyAsync(tr => tr.PatientId == appointment.PatientId &&
                                    tr.AppointmentId == appointment.AppointmentId);

                if (!exists)
                {
                    _context.TreatmentRecords.Add(new TreatmentRecord
                    {
                        PatientId = appointment.PatientId,
                        AppointmentId = appointment.AppointmentId,
                        Diagnosis = $"Seen on {date:yyyy-MM-dd} (no diagnosis yet)",
                        TreatmentDetails = "Auto-generated entry",
                        TreatmentDate = appointment.AppointmentDate
                    });
                }
            }

            await _context.SaveChangesAsync();

            var patients = appointments
                .Select(a => new
                {
                    a.Patient.PatientId,
                    a.Patient.Name,
                    a.Patient.PhoneNumber,
                    AppointmentDate = a.AppointmentDate
                })
                .OrderBy(p => p.Name)
                .ToList();

            return Ok(patients);
        }

        // -------------------- GET APPOINTMENTS BY PATIENT --------------------
[HttpGet("patients/{patientId}/appointments")]
public async Task<IActionResult> GetAppointmentsForPatient(int patientId)
{
    var appointments = await _context.Appointments
        .Where(a => a.PatientId == patientId)
        .OrderByDescending(a => a.AppointmentDate)
        .Select(a => new
        {
            appointmentId = a.AppointmentId,
            appointmentDate = a.AppointmentDate,
            status = a.Status,
            notes = a.Notes
        })
        .ToListAsync();

    return Ok(appointments);
}

// -------------------- GET PATIENTS SEEN BY DATE --------------------


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
        [HttpPut("appointments/{appointmentId}/complete")]
        public async Task<IActionResult> CompleteAppointment(int appointmentId, [FromBody] AppointmentUpdateDto dto)
        {
            var appointment = await _context.Appointments.Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null) return NotFound("Appointment not found");

            appointment.Status = "Completed";
            appointment.Notes = dto.Notes ?? appointment.Notes;

            await _context.SaveChangesAsync();

            var exists = await _context.TreatmentRecords
                .AnyAsync(tr => tr.PatientId == appointment.PatientId &&
                                tr.AppointmentId == appointment.AppointmentId);

            if (!exists)
            {
                _context.TreatmentRecords.Add(new TreatmentRecord
                {
                    PatientId = appointment.PatientId,
                    AppointmentId = appointment.AppointmentId,
                    Diagnosis = dto.Notes ?? "Seen today (no diagnosis yet)",
                    TreatmentDetails = "Auto-generated entry",
                    TreatmentDate = appointment.AppointmentDate
                });
                await _context.SaveChangesAsync();
            }

            return Ok("Appointment completed and treatment saved");
        }
    }
}
