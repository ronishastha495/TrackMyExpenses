using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackMyExpenses.Models
{
    public class User
    {
        
        public string UserName { get; set; }
        public string Password { get; set; } // This will store the hashed password
        public string Email { get; set; }
        public string? PreferredCurrency { get; set; }

    }
}
