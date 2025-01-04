using System;

namespace BudgetEase.Model
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; } // This will store the hashed password
        public string Email { get; set; }
        public string PreferredCurrency { get; set; } // Add preferred currency type
    }
}
