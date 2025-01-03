using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetEase.Model
{
    public class Debts
    {
        public string Id { get; set; }
        public decimal Amount { get; set; } // Total amount of the debt
        public decimal PaidAmount { get; set; } // Amount already paid towards the debt
        public string Creditor { get; set; } // Name of the person or entity owed money
        public DateTime DueDate { get; set; } // Date by which the debt should be paid
        public string Description { get; set; } // Debt description
    }
}
