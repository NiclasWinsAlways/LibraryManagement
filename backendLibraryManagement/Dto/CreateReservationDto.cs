namespace backendLibraryManagement.Dto
{
    // DTO used when a user reserves a book.
    public class CreateReservationDto
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
    }
}
