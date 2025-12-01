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
            public ICollection<Loan>? Loans { get; set; } // Navigation: All loans connected to this user.
    }
    }
