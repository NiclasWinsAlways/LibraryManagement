using backendLibraryManagement.Services.Interfaces;

namespace backendLibraryManagement.Services
{
    // DEV implementation: logs SMS instead of sending.
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _log;

        public SmsService(ILogger<SmsService> log)
        {
            _log = log;
        }

        public Task SendSmsAsync(string toPhoneNumber, string message)
        {
            _log.LogInformation("SMS -> {Phone}: {Message}", toPhoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
