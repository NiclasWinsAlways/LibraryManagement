using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface IReservationService
    {
        Task<List<ReservationDto>> GetAllAsync();
        Task<ReservationDto?> GetByIdAsync(int id);
        Task<(bool Success, string? Error, ReservationDto? Reservation)> CreateAsync(CreateReservationDto dto);
        Task<bool> CancelAsync(int id);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateReservationDto dto);
        Task<bool> DeleteAsync(int id);
        Task<object> GetQueueForBookAsync(int bookId);
        Task<int> ExpireReadyReservationsAsync();
    }
}
