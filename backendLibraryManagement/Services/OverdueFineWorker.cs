using backendLibraryManagement.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace backendLibraryManagement.Services
{
    public class OverdueFineWorker : BackgroundService
    {
        private readonly ILogger<OverdueFineWorker> _log;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FineOptions _opt;

        public OverdueFineWorker(
            ILogger<OverdueFineWorker> log,
            IServiceScopeFactory scopeFactory,
            IOptions<FineOptions> opt)
        {
            _log = log;
            _scopeFactory = scopeFactory;
            _opt = opt.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_opt.Enabled)
            {
                _log.LogInformation("OverdueFineWorker is disabled.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<IFineService>();

                    var changed = await svc.RunOverdueFineScanAsync();
                    if (changed > 0)
                        _log.LogInformation("OverdueFineWorker updated/created {Count} fines.", changed);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "OverdueFineWorker failed.");
                }

                var minutes = _opt.RunEveryMinutes <= 0 ? 60 : _opt.RunEveryMinutes;
                try { await Task.Delay(TimeSpan.FromMinutes(minutes), stoppingToken); } catch { }
            }
        }
    }
}
