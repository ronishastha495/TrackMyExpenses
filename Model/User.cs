using System;

namespace BudgetEase.Model
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; } // This will store the hashed password
        public string Email { get; set; }
        public string Currency { get; set; } // Add preferred currency type
    }
}
