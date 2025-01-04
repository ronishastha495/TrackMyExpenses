using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetEase.Model;

namespace BudgetEase.Services
{
    public class DebtService
    {
        private const string FilePath = @"C:\Users\asimk\OneDrive\Desktop\AD\BudgetEase\Data\debts.json"; // Path to store debt data as JSON

        // Load the list of debts for a specific user from the JSON file asynchronously
        public async Task<List<Debt>> LoadDebtsAsync(string userName)
        {
            if (!File.Exists(FilePath))
                return new List<Debt>();  // Return an empty list if no debts exist

            var json = await File.ReadAllTextAsync(FilePath);
            var allDebts = JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();

            // Filter debts based on the logged-in user's username
            var userDebts = allDebts.Where(d => d.UserName == userName).ToList();

            return userDebts;
        }

        // Save the updated list of debts to the JSON file asynchronously
        public async Task SaveDebtsAsync(List<Debt> debts)
        {
            var json = JsonSerializer.Serialize(debts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        // Add a debt for a specific user
        public async Task AddDebtAsync(Debt debt)
        {
            // Ensure Debt has all necessary properties, including Id
            if (string.IsNullOrEmpty(debt.debtId))
            {
                debt.debtId = Guid.NewGuid().ToString();  // Assign a unique ID if not already set
            }

            if (debt.Source == null)
                debt.Source = "Unknown"; // Default source if not provided

            // Ensure UserName is provided and associated with the transaction
            if (string.IsNullOrEmpty(debt.UserName))
            {
                throw new ArgumentException("UserName must be provided for the transaction.");
            }

            // Load the existing debts and add the new one
            var debts = await LoadDebtsAsync(debt.UserName);  // Pass the userName of the debt to LoadDebtsAsync
            debts.Add(debt);

            // Save the updated list of debts
            await SaveDebtsAsync(debts);
        }

        // Method to calculate the total amount of debts for a specific user
        public async Task<decimal> GetTotalDebtsAsync(string userName)
        {
            var debts = await LoadDebtsAsync(userName);  // Pass the userName to LoadDebtsAsync

            // Sum the amounts of all debts
            decimal totalDebts = debts.Sum(d => d.Amount);

            return totalDebts;
        }

        // Example of filtering debts by due date range for a specific user
        public async Task<List<Debt>> FilterDebtsAsync(string userName, DateTime startDate, DateTime endDate)
        {
            var debts = await LoadDebtsAsync(userName);  // Pass the userName to LoadDebtsAsync

            var filteredDebts = debts.Where(d => d.DueDate >= startDate && d.DueDate <= endDate).ToList();

            return filteredDebts;
        }

        // Example of searching debts by source (title/description) for a specific user
        public async Task<List<Debt>> SearchDebtsAsync(string userName, string searchQuery)
        {
            var debts = await LoadDebtsAsync(userName);  // Pass the userName to LoadDebtsAsync

            var filteredDebts = debts.Where(d => d.Source.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            return filteredDebts;
        }

        // Clear a debt (or make a payment towards it) for a specific user
        public async Task<bool> DebtPaymentAsync(string userName, string debtId, decimal paymentAmount, decimal availableCash)
        {
            if (string.IsNullOrEmpty(debtId))
                throw new ArgumentException("Debt ID cannot be null or empty.", nameof(debtId));

            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(paymentAmount));

            // Load debts (you should probably make this async if it's loading from a file or database)
            var debts = await LoadDebtsAsync(userName);  // Await the LoadDebtsAsync method
            var debt = debts.FirstOrDefault(d => d.debtId == debtId);
            if (debt == null)
                throw new InvalidOperationException("Debt not found.");

            // Check if the debt is already cleared
            if (debt.IsCleared)
                throw new InvalidOperationException("This debt is already cleared.");

            // Ensure the debt has a valid associated userName
            if (string.IsNullOrEmpty(debt.UserName))
                throw new InvalidOperationException("Username associated with the debt is null or empty.");
            // Get the current available balance for the user
            var transactionService = new TransactionService();  // Create an instance of TransactionService
            var currentBalance = await transactionService.GetAvailableBalanceForUserAsync(userName);  // Call the method on the instance
            if (paymentAmount > currentBalance)
                throw new InvalidOperationException("Insufficient balance to clear the debt.");


            // Apply the payment to the debt
            debt.PaidAmount += paymentAmount;

            // Check if the debt is fully cleared
            if (debt.PaidAmount >= debt.Amount)
            {
                debt.IsCleared = true;  // Mark the debt as cleared
                debt.PaidAmount = debt.Amount;  // Ensure the paid amount does not exceed the debt amount
                Console.WriteLine($"Debt {debtId} is fully cleared.");
            }
            

            // Record the transaction (you should use an appropriate method for this)
            await transactionService.AddTransactionAsync(new Transactions
            {
                UserName = debt.UserName,
                Amount = paymentAmount,
                TransactionType = "debt",
                Notes = $"Payment for debt {debt.Source}",
                

            });

            // Save the updated debts list (this should save the data back to your storage)
            await SaveDebtsAsync(debts);  // Use the async SaveDebtsAsync method

            return true;  // Indicate that the payment was successful
        }

        // Method to update the user's available balance (You may implement this differently based on your data storage mechanism)
      
    }
}
