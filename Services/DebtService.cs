using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TrackMyExpenses.Model;
using TrackMyExpenses.Services;

namespace TrackMyExpenses.Services
{
    public class DebtService
    {
        private const string FilePath = @"C:\Users\Ronisha Shrestha\Desktop\22085434_Ronisha Shrestha\TrackMyExpenses\LocalDB\debts.json"; // Path to store debt data as JSON

        public async Task<List<Debt>> LoadDebtsAsync(string userName)
        {
            if (!File.Exists(FilePath))
                return new List<Debt>();

            var json = await File.ReadAllTextAsync(FilePath);
            var allDebts = JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();

            // Filter debts based on the logged-in user's username
            var userDebts = allDebts.Where(d => d.UserName == userName).ToList();

            return userDebts;
        }


        public async Task SaveDebtsAsync(List<Debt> debts)
        {
            var json = JsonSerializer.Serialize(debts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        // Add a debt for a specific user
        public async Task AddDebtAsync(Debt debt)
        {

            if (string.IsNullOrEmpty(debt.debtId))
            {
                debt.debtId = Guid.NewGuid().ToString();
            }

            if (debt.Source == null)
                debt.Source = "Unknown";


            if (string.IsNullOrEmpty(debt.UserName))
            {
                throw new ArgumentException("UserName must be provided for the transaction.");
            }


            var debts = await LoadDebtsAsync(debt.UserName);
            debts.Add(debt);


            await SaveDebtsAsync(debts);
        }


        public async Task<decimal> GetTotalDebtsAsync(string userName)
        {
            var debts = await LoadDebtsAsync(userName);

            // Sum the amounts of all debts
            decimal totalDebts = debts.Sum(d => d.Amount);

            return totalDebts;
        }

        public async Task<List<Debt>> FilterDebtsAsync(string userName, DateTime startDate, DateTime endDate)
        {
            var debts = await LoadDebtsAsync(userName);  // Pass the userName to LoadDebtsAsync

            var filteredDebts = debts.Where(d => d.DueDate >= startDate && d.DueDate <= endDate).ToList();

            return filteredDebts;
        }


        public async Task<List<Debt>> SearchDebtsAsync(string userName, string searchQuery)
        {
            var debts = await LoadDebtsAsync(userName);

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


            var debts = await LoadDebtsAsync(userName);  // Await the LoadDebtsAsync method
            var debt = debts.FirstOrDefault(d => d.debtId == debtId);
            if (debt == null)
                throw new InvalidOperationException("Debt not found.");

            if (debt.IsCleared)
                throw new InvalidOperationException("This debt is already cleared.");

            if (string.IsNullOrEmpty(debt.UserName))
                throw new InvalidOperationException("Username associated with the debt is null or empty.");
            // Get the current available balance for the user
            var transactionService = new TransactionService();
            var currentBalance = await transactionService.GetTotalInflowsForUserAsync(userName);
            if (paymentAmount > currentBalance)
                throw new InvalidOperationException("Insufficient balance to clear the debt.");



            debt.PaidAmount += paymentAmount;

            // Check if the debt is fully cleared
            if (debt.PaidAmount >= debt.Amount)
            {
                debt.IsCleared = true;
                debt.PaidAmount = debt.Amount;
                Console.WriteLine($"Debt {debtId} is fully cleared.");
            }


            await SaveDebtsAsync(debts);

            return true;
        }
        // Method to get the total cleared and remaining debts for a specific user
        public async Task<(decimal clearedDebt, decimal remainingDebt)> GetDebtSummaryAsync(string userName)
        {
            var debts = await LoadDebtsAsync(userName);  // Load the user's debts

            // Calculate the total cleared and remaining debts
            decimal clearedDebt = debts.Where(d => d.IsCleared).Sum(d => d.Amount);
            decimal remainingDebt = debts.Where(d => !d.IsCleared).Sum(d => d.Amount);

            return (clearedDebt, remainingDebt);
        }

        public async Task<List<Debt>> GetPendingDebtsAsync(string userName)
        {
            var debts = await LoadDebtsAsync(userName);  // Load all debts for the user


            var pendingDebts = debts.Where(d => !d.IsCleared).ToList();

            return pendingDebts;
        }



    }
}
