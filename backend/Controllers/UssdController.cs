using Microsoft.AspNetCore.Mvc;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/Ussd")]
    public class UssdController : ControllerBase
    {
        private readonly MobileDialysisDbContext _db;

        public UssdController(MobileDialysisDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> HandleUssd(
            [FromForm] string sessionId,
            [FromForm] string serviceCode,
            [FromForm] string phoneNumber,
            [FromForm] string text)
        {
            string[] inputs = text.Split('*');
            string userResponse = inputs.Last();

            switch (inputs.Length)
            {
                case 1:
                    return Content("CON Welcome to Mobile Dialysis\n1. Book Appointment\n2. View My Appointment\n3. Cancel Appointment");

                // BOOKING
                case 2:
                    if (userResponse == "1")
                        return Content("CON Enter Appointment Date (YYYY-MM-DD)");
                    if (userResponse == "2")
                        return await ViewAppointment(phoneNumber);
                    if (userResponse == "3")
                        return await CancelAppointment(phoneNumber);

                    return Content("END Invalid choice");

                // DATE
                case 3:
                    string date = inputs[1];
                    return Content("CON Choose Truck:\n1. Truck 1\n2. Truck 2\n3. Truck 3");

                // TRUCK AND TIME SLOT
                case 4:
                    return Content("CON Choose Time Slot:\n1. 08:00–12:00\n2. 12:00–16:00\n3. 16:00–20:00\n4. 20:00–00:00");

                // CONFIRM BOOKING
                case 5:
                    string selectedDate = inputs[1];
                    int truckId = int.Parse(inputs[2]);
                    int slotOption = int.Parse(inputs[3]);

                    string timeSlot = slotOption switch
                    {
                        1 => "08:00",
                        2 => "12:00",
                        3 => "16:00",
                        4 => "20:00",
                        _ => "08:00"
                    };

                    var patient = await _db.Patients.FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber);
                    if (patient == null)
                        return Content("END You are not registered.");

                    var appt = new Appointment
                    {
                        PatientId = patient.PatientId,
                        TruckId = truckId,
                        AppointmentDate = DateTime.Parse($"{selectedDate} {timeSlot}:00")
                    };

                    _db.Appointments.Add(appt);
                    await _db.SaveChangesAsync();

                    return Content($"END Booking Confirmed for {selectedDate} at {timeSlot}");

                default:
                    return Content("END Invalid Input");
            }
        }

        private async Task<IActionResult> ViewAppointment(string phone)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.PhoneNumber == phone);
            if (patient == null)
                return Content("END Not registered.");

            var appt = await _db.Appointments
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.AppointmentDate)
                .FirstOrDefaultAsync();

            if (appt == null)
                return Content("END You have no appointments.");

            return Content($"END Appointment:\n{appt.AppointmentDate}");
        }

        private async Task<IActionResult> CancelAppointment(string phone)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.PhoneNumber == phone);
            if (patient == null)
                return Content("END Not registered.");

            var appt = await _db.Appointments
                .Where(a => a.PatientId == patient.PatientId)
                .FirstOrDefaultAsync();

            if (appt == null)
                return Content("END No appointment to cancel.");

            _db.Appointments.Remove(appt);
            await _db.SaveChangesAsync();

            return Content("END Appointment cancelled.");
        }
    }
}
