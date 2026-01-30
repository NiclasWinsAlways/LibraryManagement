using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetForBookAsync(int bookId);
        Task<BookRatingSummaryDto> GetRatingSummaryAsync(int bookId);

        Task<(bool Success, string? Error, ReviewDto? Review)> CreateAsync(int bookId, CreateReviewDto dto);
        Task<(bool Success, string? Error)> UpdateAsync(int reviewId, UpdateReviewDto dto);
        Task<bool> DeleteAsync(int reviewId);
    }
}
