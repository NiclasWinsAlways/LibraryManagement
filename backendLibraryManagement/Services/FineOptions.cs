namespace backendLibraryManagement.Services
{
    public class FineOptions
    {
        public bool Enabled { get; set; } = true;

        // DKK per day overdue
        public decimal DailyRate { get; set; } = 10m;

        // Max fine per loan
        public decimal MaxFine { get; set; } = 300m;

        // Worker interval
        public int RunEveryMinutes { get; set; } = 60;
    }
}
