using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface ILoanService
    {
        Task<List<LoanDto>> GetAllAsync();
        Task<LoanDto?> GetByIdAsync(int id);
        Task<(bool Success, string? Error, LoanDto? Loan)> CreateAsync(CreateLoanDto dto);
        Task<bool> ReturnLoanAsync(int loanId);
        Task<(bool Success, string? Error, LoanDto? Loan)> ExtendLoanAsync(int loanId, int days);

        // (du havde den før - valgfri om du stadig vil bruge den)
        Task CheckDueLoansAsync();
    }
}
