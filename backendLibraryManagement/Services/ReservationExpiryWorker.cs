using backendLibraryManagement.Services.Interfaces;

namespace backendLibraryManagement.Services
{
    // Runs periodically and expires Ready reservations past ExpiresAt.
    public class ReservationExpiryWorker : BackgroundService
    {
        private readonly ILogger<ReservationExpiryWorker> _log;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReservationExpiryWorker(ILogger<ReservationExpiryWorker> log, IServiceScopeFactory scopeFactory)
            => (_log, _scopeFactory) = (log, scopeFactory);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<IReservationService>();
                    var expired = await svc.ExpireReadyReservationsAsync();

                    if (expired > 0)
                        _log.LogInformation("Expired {Count} reservations.", expired);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "ReservationExpiryWorker failed.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
                catch { }
            }
        }
    }
}
