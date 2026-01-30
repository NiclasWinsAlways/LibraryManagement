namespace backendLibraryManagement.Dto
{
    public class LoansTrendPointDto
    {
        public string Date { get; set; } = ""; // "yyyy-MM-dd"
        public int LoansStarted { get; set; }
        public int LoansReturned { get; set; }
    }
}
