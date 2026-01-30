using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(int dueWithinDays = 2);
        Task<List<TopBookDto>> GetTopBooksAsync(int days = 30, int take = 10);
        Task<List<LoansTrendPointDto>> GetLoansTrendAsync(int days = 30);
    }
}
