namespace backendLibraryManagement.Model
{
    // Represents an application user.
    // Can be a normal borrower ("Låner") or an administrator.
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = ""; // Full name of the user.
        public string Email { get; set; } = ""; // Email address, used for login.
        public string PasswordHash { get; set; } = ""; // Hashed password (never store plaintext passwords).
        public string Role { get; set; } = "Låner"; // User role (e.g. "Admin" or "Låner").

        // NEW: profile fields
        public string? PhoneNumber { get; set; }
        public bool SmsOptIn { get; set; } = false;
        public bool EmailOptIn { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Loan>? Loans { get; set; } // Navigation: All loans connected to this user.
    }
}
