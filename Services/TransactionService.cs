using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetEase.Model;

namespace BudgetEase.Services
{
    public class TransactionService
    {
        private const string FilePath = @"C:\Users\asimk\OneDrive\Desktop\AD\BudgetEase\Data\transactions.json"; // Path to store transaction data as JSON

        // Load the list of transactions from the JSON file asynchronously
        public async Task<List<Transactions>> LoadTransactionsAsync()
        {
            if (!File.Exists(FilePath))
                return new List<Transactions>();  // Return an empty list if no transactions exist

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<Transactions>>(json) ?? new List<Transactions>();
        }

        // Save the updated list of transactions to the JSON file asynchronously
        public async Task SaveTransactionsAsync(List<Transactions> transactions)
        {
            var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        // Add a new transaction to the list and save it
        public async Task AddTransactionAsync(Transactions transaction)
        {
            // Ensure the Category is set (if null, default to "credit" or "debit" as appropriate)
            if (transaction.Category == null)
            {
                // Check if it's an inflow or outflow based on the transaction amount
                if (transaction.Amount > 0)
                {
                    // Set the category for positive amounts (inflows)
                    // For now, we can default to "credit" but you could adjust this logic as needed
                    transaction.Category = "credit"; // Inflows (e.g., salary, gain)
                }
                else
                {
                    // Set the category for negative amounts (outflows)
                    // You can set it as "debit", "expenses", or "spending" based on your requirement
                    transaction.Category = "debit"; // Outflows (e.g., expenses, spending)
                }
            }

            // Check if it's an outflow (debit) and ensure sufficient balance is available
            if (transaction.Category == "debit" && transaction.Amount < 0)
            {
                // Assuming we have a method to check balance - you should define balance management in your app.
                decimal availableBalance = await GetAvailableBalanceAsync(); // Implement balance retrieval
                if (Math.Abs(transaction.Amount) > availableBalance)
                {
                    throw new InvalidOperationException("Insufficient balance for this transaction.");
                }
            }

            // If it's a debt transaction, set the category explicitly
            if (transaction.Category == "debt" && transaction.Amount < 0)
            {
                // Handle debt logic - potentially check if debt amount is valid or if due date is set
            }

            // Load the existing transactions and add the new one
            var transactions = await LoadTransactionsAsync();
            transactions.Add(transaction);

            // Save the updated list of transactions
            await SaveTransactionsAsync(transactions);
        }


        // Method to get available balance from transactions
        public async Task<decimal> GetAvailableBalanceAsync()
        {
            var transactions = await LoadTransactionsAsync();

            // Calculate the balance by summing up credits and debits
            decimal balance = transactions
                .Where(t => t.Category == "credit")
                .Sum(t => t.Amount) - transactions
                .Where(t => t.Category == "debit")
                .Sum(t => t.Amount);

            return balance;
        }

        // Remove a transaction by its ID from the list and save the changes
        public async Task RemoveTransactionAsync(string transactionId)
        {
            var transactions = await LoadTransactionsAsync();
            var transactionToRemove = transactions.FirstOrDefault(t => t.Id == transactionId);
            if (transactionToRemove != null)
            {
                transactions.Remove(transactionToRemove);
                await SaveTransactionsAsync(transactions);
            }
        }

        // Example of updating an existing transaction
        public async Task UpdateTransactionAsync(string transactionId, Transactions updatedTransaction)
        {
            var transactions = await LoadTransactionsAsync();
            var existingTransaction = transactions.FirstOrDefault(t => t.Id == transactionId);

            if (existingTransaction != null)
            {
                // Update existing transaction properties
                existingTransaction.Amount = updatedTransaction.Amount;
                existingTransaction.Category = updatedTransaction.Category;
                existingTransaction.Description = updatedTransaction.Description;
                existingTransaction.Date = updatedTransaction.Date;
                existingTransaction.Notes = updatedTransaction.Notes;
                existingTransaction.Tags = updatedTransaction.Tags ?? new List<string>(); // Ensure tags are set (or empty if null)
                existingTransaction.IsDebt = updatedTransaction.IsDebt;
                existingTransaction.DebtAmount = updatedTransaction.DebtAmount;
                existingTransaction.DueDate = updatedTransaction.DueDate;
                existingTransaction.IsPendingDebt = updatedTransaction.IsPendingDebt;
                existingTransaction.AmountPaidToClearDebt = updatedTransaction.AmountPaidToClearDebt;
                existingTransaction.BalanceRequired = updatedTransaction.BalanceRequired;

                // Save the updated list of transactions
                await SaveTransactionsAsync(transactions);
            }
        }

        // Example of filtering transactions by category (credit, debit, debt) and date range
        public async Task<List<Transactions>> FilterTransactionsAsync(string category, DateTime startDate, DateTime endDate)
        {
            var transactions = await LoadTransactionsAsync();

            var filteredTransactions = transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase) &&
                                                                t.Date >= startDate &&
                                                                t.Date <= endDate).ToList();

            return filteredTransactions;
        }

        // Example of searching transactions by title (description)
        public async Task<List<Transactions>> SearchTransactionsAsync(string searchQuery)
        {
            var transactions = await LoadTransactionsAsync();

            var filteredTransactions = transactions.Where(t => t.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            return filteredTransactions;
        }
    }
}
