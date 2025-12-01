using Microsoft.Extensions.Options;

namespace backendLibraryManagement.Services
{
    // Background worker that runs once per day at a configured hour.
    // Its job is to scan for loans that are due soon and create notifications for the users.
    public class DueReminderWorker: BackgroundService
    {
        private readonly ILogger<DueReminderWorker> _log;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ReminderOptions _opt;

        public DueReminderWorker(
            ILogger<DueReminderWorker> log,
            IServiceScopeFactory scopeFactory,
            IOptions<ReminderOptions> opt)
        {
            _log = log;
            _scopeFactory = scopeFactory;
            _opt = opt.Value;
        }

        // Main background loop
        protected override async Task ExecuteAsync(CancellationToken Stoppingtoken)
        {
            if (!_opt.Enabled)
            {
                _log.LogInformation("DueReminderWorker is disabled.");
                return;
            }

            // Convert based on configured timezone.
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_opt.Timezone);
            while (!Stoppingtoken.IsCancellationRequested)
            {
                // Current local time
                var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);

                // Next run time (today at DailyHour, otherwise tomorrow)
                var nextLocal = new DateTime(
                    nowLocal.Year, nowLocal.Month, nowLocal.Day,
                    _opt.DailyHour, 0, 0
                );

                if (nowLocal >= nextLocal)
                    nextLocal = nextLocal.AddDays(1);

                // Calculate delay in UTC
                var delay =TimeZoneInfo.ConvertTimeToUtc(nextLocal,tz) - DateTime.UtcNow;
                if(delay<TimeSpan.FromMinutes(1))
                    delay=TimeSpan.FromMinutes(1);
                _log.LogInformation("DueReminderWorker sleeping {Delay:g} until {Next}.", delay, nextLocal);

                try { await Task.Delay(delay, Stoppingtoken); } catch { }

                if (Stoppingtoken.IsCancellationRequested) break;

                // Run notification job inside scoped services
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notifier = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    await notifier.NotifyUpcomingDueDatesAsync();
                }catch(Exception ex)
                {
                    _log.LogError(ex, "DueReminderWorker failed.");
                }
            }
        }
    }
}
