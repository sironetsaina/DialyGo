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

        [HttpGet("assigned-truck/{doctorId}")]
        public async Task<IActionResult> GetAssignedTruck(int doctorId)
        {
            var assignment = await _context.TruckStaffAssignments
                .Include(a => a.Truck)
                .FirstOrDefaultAsync(a => a.StaffId == doctorId && a.Role == "Doctor");

            if (assignment == null) return NotFound("No assigned truck found.");

            return Ok(new
            {
                assignment.Truck.TruckId,
                assignment.Truck.LicensePlate,
                assignment.Truck.CurrentLocation,
                assignment.Truck.Capacity
            });
        }

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

        [HttpPost("patients/{patientId}/treatment")]
        public async Task<IActionResult> AddTreatmentRecord(int patientId, [FromBody] TreatmentSummaryDto dto)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists) return NotFound("Patient not found.");

            var treatment = new TreatmentRecord
            {
                PatientId = patientId,
                TreatmentDate = DateTime.UtcNow,
                Diagnosis = dto.Diagnosis,
                TreatmentDetails = dto.TreatmentDetails
            };

            _context.TreatmentRecords.Add(treatment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Treatment record added successfully.",
                TreatmentId = treatment.TreatmentId
            });
        }

        [HttpPut("patients/{patientId}/diagnosis")]
        public async Task<IActionResult> UpdatePatientDiagnosis(int patientId, [FromBody] string newDiagnosis)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return NotFound("Patient not found.");

            patient.MedicalHistory = newDiagnosis;
            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Patient diagnosis updated successfully.");
        }

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
    }
}
