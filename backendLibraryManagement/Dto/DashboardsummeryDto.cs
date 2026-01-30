namespace backendLibraryManagement.Dto
{
    public class DashboardSummaryDto
    {
        public int TotalBooks { get; set; }
        public int TotalCopies { get; set; }
        public int CopiesAvailable { get; set; }

        public int ActiveLoans { get; set; }
        public int DueSoonLoans { get; set; }     // due within X days
        public int OverdueLoans { get; set; }

        public int ActiveReservations { get; set; }
        public int ReadyReservations { get; set; }
    }
}
