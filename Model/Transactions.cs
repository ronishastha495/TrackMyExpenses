using System;
using System.Collections.Generic;

namespace BudgetEase.Model
{
    public class Transactions
    {
        public string Id { get; set; } // Unique transaction ID

        public decimal Amount { get; set; } // The amount of money involved

        public string Category { get; set; } // Inflow, Outflow, Debt

        public string Description { get; set; } // Description of the transaction (e.g., "Salary", "Rent")

        public DateTime Date { get; set; } // Transaction date

        // New field to store optional notes for each transaction
        public string Notes { get; set; } // Optional notes for the transaction

        // New field to store tags for categorizing transactions (can be a list of predefined or custom tags)
        public List<string> Tags { get; set; } // Tags associated with the transaction (e.g., "Rent", "Groceries", "Salary")

        // For debt tracking
        public bool IsDebt { get; set; } // True if the transaction is related to a debt
        public decimal? DebtAmount { get; set; } // The amount of debt if it's a debt transaction (nullable, only applicable to debt transactions)
        public DateTime? DueDate { get; set; } // Due date for the debt (nullable, only applicable to debt transactions)

        // For tracking pending debts
        public bool IsPendingDebt { get; set; } // True if the debt is still pending (not yet cleared)

        // Additional property for clearing debt from inflows
        public decimal? AmountPaidToClearDebt { get; set; } // Amount paid to clear a debt (only relevant for debt payments)

        // This field will help with validation for outflows (ensure balance before processing outflows)
        public decimal? BalanceRequired { get; set; } // Balance required for cash outflows (nullable, only applicable to outflows)

        // Add this new field for TransactionType
        public string TransactionType { get; set; } // "Inflow", "Outflow", or "Debt"
    }
}
