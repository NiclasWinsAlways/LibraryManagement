    namespace backendLibraryManagement.Model
    {
        public class User
        {
            public int Id { get; set; }

            public string Name { get; set; } = "";
            public string Email { get; set; } = "";

            // Hashed password – aldrig gem ren tekst!
            public string PasswordHash { get; set; } = "";

            // Rolle: "Bibliotekar" eller "Låner"
            public string Role { get; set; } = "Låner";

            // Relation: en bruger kan have flere lån
            public ICollection<Loan>? Loans { get; set; }
        }
    }
