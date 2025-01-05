using System.Text.Json;

namespace TrackMyExpenses.Services
{
    public class TransactionService
    {
        private const string FilePath = @"C:\Users\Ronisha Shrestha\Desktop\LocalDB\transactions.json";

        // Load transactions from the JSON file
        public async Task<List<Transactions>> LoadTransactionsAsync()
        {
            if (!File.Exists(FilePath))
                return new List<Transactions>();

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<Transactions>>(json) ?? new List<Transactions>();
        }

        // Save transactions to the JSON file
        public async Task SaveTransactionsAsync(List<Transactions> transactions)
        {
            var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        // Add a new transaction
        public async Task AddTransactionAsync(Transactions transaction)
        {
            // Ensure UserName is provided and associated with the transaction
            if (string.IsNullOrEmpty(transaction.UserName))
            {
                throw new ArgumentException("UserName must be provided for the transaction.");
            }

            // Set the category if it's null based on the amount
            if (transaction.Category == null)
            {
                transaction.Category = transaction.Amount > 0 ? "credit" : "debit";
            }

            // If it's a debit transaction and the amount is negative, check the available balance
            if (transaction.Category == "debit" && transaction.Amount < 0)
            {
                decimal availableBalance = await GetAvailableBalanceForUserAsync(transaction.UserName);
                if (Math.Abs(transaction.Amount) > availableBalance)
                {
                    throw new InvalidOperationException("Insufficient balance for this transaction.");
                }
            }

            // Load current transactions and add the new one
            var transactions = await LoadTransactionsAsync();
            transactions.Add(transaction);

            // Save the updated transactions back to the file
            await SaveTransactionsAsync(transactions);
        }

        // Calculate total inflows for a specific user
        public async Task<decimal> GetTotalInflowsForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "credit")
                .Sum(t => t.Amount);
        }

        // Calculate total outflows for a specific user
        public async Task<decimal> GetTotalOutflowsForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "debit")
                .Sum(t => Math.Abs(t.Amount));
        }

        // Get available balance for a specific user
        public  async Task<decimal> GetAvailableBalanceForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();

            // Calculate total credit and total debit for a specific user
            decimal totalCredit = transactions
                .Where(t => t.UserName == userName && t.Category == "credit")
                .Sum(t => t.Amount);
            decimal totalDebit = transactions
                .Where(t => t.UserName == userName && t.Category == "debit")
                .Sum(t => t.Amount);

            // Return the available balance for the user
            return totalCredit - totalDebit;
        }

        // Remove a transaction by user
        public async Task RemoveTransactionForUserAsync(string transactionId, string userName)
        {
            var transactions = await LoadTransactionsAsync();
            var transactionToRemove = transactions
                .FirstOrDefault(t => t.Id == transactionId && t.UserName == userName);
            if (transactionToRemove != null)
            {
                transactions.Remove(transactionToRemove);
                await SaveTransactionsAsync(transactions);
            }
        }

        // Update a transaction for a specific user
        public async Task UpdateTransactionForUserAsync(string transactionId, string userName, Transactions updatedTransaction)
        {
            var transactions = await LoadTransactionsAsync();
            var existingTransaction = transactions
                .FirstOrDefault(t => t.Id == transactionId && t.UserName == userName);

            if (existingTransaction != null)
            {
                existingTransaction.Amount = updatedTransaction.Amount;
                existingTransaction.Category = updatedTransaction.Category;
                existingTransaction.Description = updatedTransaction.Description;
                existingTransaction.Date = updatedTransaction.Date;
                existingTransaction.Notes = updatedTransaction.Notes;
                existingTransaction.Tags = updatedTransaction.Tags ?? new List<string>();
                existingTransaction.IsDebt = updatedTransaction.IsDebt;
                existingTransaction.DebtAmount = updatedTransaction.DebtAmount;
                existingTransaction.DueDate = updatedTransaction.DueDate;
                existingTransaction.IsPendingDebt = updatedTransaction.IsPendingDebt;
                existingTransaction.AmountPaidToClearDebt = updatedTransaction.AmountPaidToClearDebt;
                existingTransaction.BalanceRequired = updatedTransaction.BalanceRequired;

                await SaveTransactionsAsync(transactions);
            }
        }

        public async Task<List<Transactions>> FilterTransactionsForUserAsync(string userName, string? category = null, List<string>? tags = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Load all transactions asynchronously
            var transactions = await LoadTransactionsAsync();

            // Filter the transactions based on the provided criteria and userName
            var filteredTransactions = transactions
                .Where(t => t.UserName == userName) // Ensure transactions belong to the user
                                                    // Filter by category if provided
                .Where(t => string.IsNullOrEmpty(category) || t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                // Filter by tags if provided
                .Where(t => tags == null || !tags.Any() || tags.All(tag => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                // Filter by start date if provided
                .Where(t => !startDate.HasValue || t.Date >= startDate.Value)
                // Filter by end date if provided
                .Where(t => !endDate.HasValue || t.Date <= endDate.Value)
                .ToList();

            return filteredTransactions;
        }

        // Search transactions for a specific user by description
        public async Task<List<Transactions>> SearchTransactionsForUserAsync(string userName, string searchQuery)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}

