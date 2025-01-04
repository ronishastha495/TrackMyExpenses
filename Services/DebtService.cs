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

        // Load the list of debts from the JSON file asynchronously
        public async Task<List<Debt>> LoadDebtsAsync()
        {
            if (!File.Exists(FilePath))
                return new List<Debt>();  // Return an empty list if no debts exist

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
        }

        // Save the updated list of debts to the JSON file asynchronously
        public async Task SaveDebtsAsync(List<Debt> debts)
        {
            var json = JsonSerializer.Serialize(debts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        // Add a new debt to the list and save it
        public async Task AddDebtAsync(Debt debt)
        {
            // Ensure Debt has all necessary properties
            if (debt.Source == null)
                debt.Source = "Unknown"; // Default source if not provided

            // Load the existing debts and add the new one
            var debts = await LoadDebtsAsync();
            debts.Add(debt);

            // Save the updated list of debts
            await SaveDebtsAsync(debts);
        }

        // Remove a debt by its ID from the list and save the changes
        public async Task RemoveDebtAsync(int debtId)
        {
            var debts = await LoadDebtsAsync();
            var debtToRemove = debts.FirstOrDefault(d => d.Id == debtId);
            if (debtToRemove != null)
            {
                debts.Remove(debtToRemove);
                await SaveDebtsAsync(debts);
            }
        }

        // Example of updating an existing debt
        public async Task UpdateDebtAsync(int debtId, Debt updatedDebt)
        {
            var debts = await LoadDebtsAsync();
            var existingDebt = debts.FirstOrDefault(d => d.Id == debtId);

            if (existingDebt != null)
            {
                // Update existing debt properties
                existingDebt.Source = updatedDebt.Source;
                existingDebt.Amount = updatedDebt.Amount;
                existingDebt.DueDate = updatedDebt.DueDate;
                existingDebt.Notes = updatedDebt.Notes;
                existingDebt.Tags = updatedDebt.Tags;
                existingDebt.IsCleared = updatedDebt.IsCleared;

                // Save the updated list of debts
                await SaveDebtsAsync(debts);
            }
        }

        // Example of filtering debts by due date range
        public async Task<List<Debt>> FilterDebtsAsync(DateTime startDate, DateTime endDate)
        {
            var debts = await LoadDebtsAsync();

            var filteredDebts = debts.Where(d => d.DueDate >= startDate && d.DueDate <= endDate).ToList();

            return filteredDebts;
        }

        // Example of searching debts by source (title/description)
        public async Task<List<Debt>> SearchDebtsAsync(string searchQuery)
        {
            var debts = await LoadDebtsAsync();

            var filteredDebts = debts.Where(d => d.Source.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            return filteredDebts;
        }

        // Example of clearing a debt (marking it as cleared)
        public async Task ClearDebtAsync(int debtId)
        {
            var debts = await LoadDebtsAsync();
            var debtToClear = debts.FirstOrDefault(d => d.Id == debtId);
            if (debtToClear != null && !debtToClear.IsCleared)
            {
                debtToClear.IsCleared = true;
                await SaveDebtsAsync(debts);
            }
            else
            {
                throw new InvalidOperationException("Debt is already cleared or doesn't exist.");
            }
        }
    }
}
