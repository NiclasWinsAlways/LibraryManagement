namespace backendLibraryManagement.Dto
{
    // DTO representing reservation data returned to the client.
    public class ReservationDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; } // The date the reservation was made.
        public string Status { get; set; } = ""; // Reservation state (Active / Cancelled / Fulfilled)
    }
}
