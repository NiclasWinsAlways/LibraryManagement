namespace backendLibraryManagement.Dto
{
    // DTO used when updating reservation status.
    // Status examples: Active / Cancelled / Fulfilled
    public class UpdateReservationDto
    {
        public string? Status { get; set; }
    }
}
