using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface IFineService
    {
        Task<List<FineDto>> GetFinesForUserAsync(int userId);

        // Creates/updates fines for overdue loans (MVP)
        Task<int> RunOverdueFineScanAsync();

        // Pays fine and returns receipt
        Task<(bool Success, string? Error, ReceiptDto? Receipt)> PayFineAsync(int fineId, int userId);

        Task<ReceiptDto?> GetReceiptAsync(int receiptId);
        Task<List<ReceiptDto>> GetReceiptsForUserAsync(int userId);
    }
}
