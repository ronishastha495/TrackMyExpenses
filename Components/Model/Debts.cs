using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackMyExpenses.Model
{
    public class Debt
    {
        public string debtId { get; set; }
        public string? UserName { get; set; }
        public string Source { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; } = 0;

        public DateTime DueDate { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }  // You could store this as a comma-separated string or create a separate table for tags.
        public bool IsCleared { get; set; } // Track if the debt is cleared or not
    }

}
