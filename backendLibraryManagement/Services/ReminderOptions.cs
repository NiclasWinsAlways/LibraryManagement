namespace backendLibraryManagement.Services
{
    public class ReminderOptions
    {
        public bool Enabled { get; set; } = true;
        public int DailyHour { get; set; } = 9;
        public string Timezone { get; set; } = "Europe/Copenhagen";
        public int WindowMinDays { get; set; } = 1;
        public int WindowMaxDays { get; set; } = 2;
    }
}
