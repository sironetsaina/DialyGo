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

        // ---------------------------------------------------------
        // 1️⃣ REGISTER NEW PATIENT
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // 2️⃣ GET SINGLE PATIENT BY ID
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // 3️⃣ UPDATE PATIENT DETAILS (TRIAGE + BASIC INFO)
        // ---------------------------------------------------------
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

            return NoContent();
        }

        // ---------------------------------------------------------
        // 4️⃣ GET TREATMENT SUMMARY FOR A PATIENT
        // ---------------------------------------------------------
        [HttpGet("patients/{id}/treatment-summary")]
        public async Task<ActionResult<IEnumerable<TreatmentSummaryDto>>> GetTreatmentSummary(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.TreatmentRecords)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound();

            var summary = patient.TreatmentRecords.Select(tr => new TreatmentSummaryDto
            {
                TreatmentId = tr.TreatmentId,
                TreatmentDate = tr.TreatmentDate,
                Diagnosis = tr.Diagnosis,
                TreatmentDetails = tr.TreatmentDetails
            }).ToList();

            return Ok(summary);
        }

        // ---------------------------------------------------------
        // 5️⃣ GET NURSE BY ID
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // 6️⃣ GET ALL PATIENTS (No truck filtering)
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // 7️⃣ GET FULL HEALTH DETAILS (TREATMENTS + APPOINTMENTS)
        // ---------------------------------------------------------
        [HttpGet("patients/{id}/health-details")]
        public async Task<ActionResult<object>> GetPatientHealthDetails(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.TreatmentRecords)
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound();

            return Ok(new
            {
                Patient = new
                {
                    patient.PatientId,
                    patient.Name,
                    patient.Gender,
                    patient.DateOfBirth,
                    patient.PhoneNumber,
                    patient.Email,
                    patient.Address,
                    patient.MedicalHistory
                },
                TreatmentHistory = patient.TreatmentRecords.Select(t => new
                {
                    t.TreatmentId,
                    t.TreatmentDate,
                    t.Diagnosis,
                    t.TreatmentDetails
                }),
                Appointments = patient.Appointments.Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentDate,
                    a.Status,
                    a.Notes
                })
            });
        }
    }
}
