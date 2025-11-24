using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly MobileDialysisDbContext _context;

        public DoctorController(MobileDialysisDbContext context)
        {
            _context = context;
        }

        // -------------------- GET DOCTOR --------------------
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetDoctor(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found.");

            return Ok(new
            {
                doctor.DoctorId,
                doctor.Name,
                doctor.Email,
                doctor.Specialization,
                doctor.PhoneNumber
            });
        }

        // -------------------- GET PATIENT --------------------
        [HttpGet("patients/{patientId}")]
        public async Task<IActionResult> GetPatient(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return NotFound("Patient not found.");

            return Ok(new
            {
                patient.PatientId,
                patient.Name,
                patient.Gender,
                patient.DateOfBirth,
                patient.PhoneNumber,
                patient.Email,
                patient.Address,
                patient.MedicalHistory
            });
        }

        // -------------------- GET PATIENT TREATMENTS --------------------
        [HttpGet("patients/{patientId}/treatments")]
        public async Task<IActionResult> GetPatientTreatmentSummary(int patientId)
        {
            var treatments = await _context.TreatmentRecords
                .Where(t => t.PatientId == patientId)
                .Select(t => new
                {
                    t.TreatmentId,
                    t.TreatmentDate,
                    t.Diagnosis,
                    t.TreatmentDetails
                })
                .ToListAsync();

            return Ok(treatments);
        }

        // -------------------- UPDATE PATIENT DIAGNOSIS + ADD TREATMENT --------------------
        [HttpPut("patients/{patientId}/update")]
        public async Task<IActionResult> UpdatePatientDiagnosisAndTreatment(int patientId, [FromBody] TreatmentSummaryDto dto)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return NotFound("Patient not found.");

            if (!string.IsNullOrWhiteSpace(dto.Diagnosis))
                patient.MedicalHistory = dto.Diagnosis;

            _context.Entry(patient).State = EntityState.Modified;

            if (!string.IsNullOrWhiteSpace(dto.TreatmentDetails))
            {
                var latestAppointment = await _context.Appointments
                    .Where(a => a.PatientId == patientId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (latestAppointment == null)
                    return BadRequest("Cannot add treatment: no appointment exists for patient.");

                var treatment = new TreatmentRecord
                {
                    PatientId = patientId,
                    AppointmentId = latestAppointment.AppointmentId,
                    Diagnosis = dto.Diagnosis ?? patient.MedicalHistory,
                    TreatmentDetails = dto.TreatmentDetails,
                    TreatmentDate = DateTime.UtcNow
                };
                _context.TreatmentRecords.Add(treatment);
            }

            await _context.SaveChangesAsync();
            return Ok("Patient diagnosis and treatment updated successfully.");
        }

        // -------------------- UPDATE EXISTING TREATMENT --------------------
        [HttpPut("treatment/{treatmentId}")]
        public async Task<IActionResult> UpdateTreatmentDetails(int treatmentId, [FromBody] TreatmentSummaryDto dto)
        {
            var treatment = await _context.TreatmentRecords.FindAsync(treatmentId);
            if (treatment == null)
                return NotFound("Treatment record not found.");

            treatment.Diagnosis = dto.Diagnosis ?? treatment.Diagnosis;
            treatment.TreatmentDetails = dto.TreatmentDetails ?? treatment.TreatmentDetails;

            _context.Entry(treatment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Treatment record updated successfully.");
        }

        // -------------------- PATIENTS SEEN TODAY --------------------
        [HttpGet("patients-seen-today")]
        public async Task<IActionResult> GetPatientsSeenToday()
        {
            var today = DateTime.UtcNow.Date;

            var patients = await _context.TreatmentRecords
                .Where(t => t.TreatmentDate.HasValue && t.TreatmentDate.Value.Date == today)
                .Include(t => t.Patient)
                .Select(t => new
                {
                    t.Patient.PatientId,
                    t.Patient.Name,
                    t.Diagnosis,
                    t.TreatmentDate
                })
                .ToListAsync();

            return Ok(patients);
        }

        // -------------------- PATIENTS SEEN ON SPECIFIC DATE --------------------
        [HttpGet("patients-seen")]
        public async Task<IActionResult> GetPatientsSeenOnDate([FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out DateTime parsedDate))
                return BadRequest("Invalid date format.");

            var patients = await _context.TreatmentRecords
                .Where(t => t.TreatmentDate.HasValue && t.TreatmentDate.Value.Date == parsedDate.Date)
                .Include(t => t.Patient)
                .Select(t => new
                {
                    t.Patient.PatientId,
                    t.Patient.Name,
                    t.Diagnosis,
                    t.TreatmentDate
                })
                .ToListAsync();

            return Ok(patients);
        }
    }
}
