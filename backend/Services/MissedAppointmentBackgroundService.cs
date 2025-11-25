using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using backend.Models;

namespace backend.Services
{
    public class MissedAppointmentBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // run every hour

        public MissedAppointmentBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run immediately once, then every hour
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndMarkMissedAppointments(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Replace with real logging as needed
                    Console.WriteLine($"MissedAppointmentsService error: {ex.Message}");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CheckAndMarkMissedAppointments(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MobileDialysisDbContext>();
            var sms = scope.ServiceProvider.GetRequiredService<SmsService>();

            var now = DateTime.UtcNow;

            var missedAppointments = await db.Appointments
                .Where(a => a.Status == "Scheduled" && a.AppointmentDate < now)
                .ToListAsync(cancellationToken);

            if (!missedAppointments.Any()) return;

            foreach (var appt in missedAppointments)
            {
                // mark as missed
                appt.Status = "Missed";

                var patient = await db.Patients.FindAsync(new object[] { appt.PatientId }, cancellationToken);
                if (patient == null) continue;

                string message = $"You missed your appointment on {appt.AppointmentDate:dddd, MMM dd yyyy HH:mm}. Please rebook.";

                db.Smsnotifications.Add(new Smsnotification
                {
                    PatientId = appt.PatientId,
                    Message = message,
                    SentAt = DateTime.UtcNow,
                    SentBy = "System",
                    SenderId = 0,
                    SenderRole = "Sys"
                });

                try
                {
                    await sms.SendSms(patient.PhoneNumber, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SMS sending failed for patient {patient.PatientId}: {ex.Message}");
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
