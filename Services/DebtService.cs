using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BudgetEase.Model;

namespace BudgetEase.Services
{
    public class DebtService
    {
        private const string FilePath = @"C:\Users\asimk\OneDrive\Desktop\AD\BudgetEase\Data\debts.json"; // Path to store debt data as JSON

        public List<Debts> LoadDebts()
        {
            if (!File.Exists(FilePath))
                return new List<Debts>();  // Return an empty list if no debts exist

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Debts>>(json) ?? new List<Debts>();
        }

        public void SaveDebts(List<Debts> debts)
        {
            var json = JsonSerializer.Serialize(debts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public void AddDebt(Debts debt)
        {
            var debts = LoadDebts();
            debts.Add(debt);
            SaveDebts(debts);
        }

        public void RemoveDebt(string debtId)
        {
            var debts = LoadDebts();
            var debtToRemove = debts.FirstOrDefault(d => d.Id == debtId);
            if (debtToRemove != null)
            {
                debts.Remove(debtToRemove);
                SaveDebts(debts);
            }
        }

        public void UpdateDebt(Debts debt)
        {
            var debts = LoadDebts();
            var debtToUpdate = debts.FirstOrDefault(d => d.Id == debt.Id);
            if (debtToUpdate != null)
            {
                debtToUpdate.Amount = debt.Amount;
                debtToUpdate.PaidAmount = debt.PaidAmount;
                debtToUpdate.Creditor = debt.Creditor;
                debtToUpdate.DueDate = debt.DueDate;
                debtToUpdate.Description = debt.Description;
                SaveDebts(debts);
            }
        }
    }
}
