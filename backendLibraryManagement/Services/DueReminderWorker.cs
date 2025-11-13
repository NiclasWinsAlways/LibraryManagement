using Microsoft.Extensions.Options;

namespace backendLibraryManagement.Services
{
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

        protected override async Task ExecuteAsync(CancellationToken Stoppingtoken)
        {
            if (!_opt.Enabled)
            {
                _log.LogInformation("DueReminderWorker is disabled.");
                return;
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById(_opt.Timezone);
            while (!Stoppingtoken.IsCancellationRequested)
            {
                var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);

                var nextLocal = new DateTime(
                    nowLocal.Year, nowLocal.Month, nowLocal.Day,
                    _opt.DailyHour, 0, 0
                );

                if (nowLocal >= nextLocal)
                    nextLocal = nextLocal.AddDays(1);

                var delay =TimeZoneInfo.ConvertTimeToUtc(nextLocal,tz) - DateTime.UtcNow;
                if(delay<TimeSpan.FromMinutes(1))
                    delay=TimeSpan.FromMinutes(1);
                _log.LogInformation("DueReminderWorker sleeping {Delay:g} until {Next}.", delay, nextLocal);

                try { await Task.Delay(delay, Stoppingtoken); } catch { }

                if (Stoppingtoken.IsCancellationRequested) break;
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
