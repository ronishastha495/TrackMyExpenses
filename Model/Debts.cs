using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetEase.Model
{
    public class Debt
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }  // You could store this as a comma-separated string or create a separate table for tags.
        public bool IsCleared { get; set; } // Track if the debt is cleared or not
    }

}
