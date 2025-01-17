using System.Text.Json;
using TrackMyExpenses.Services;
using Microsoft.AspNetCore.Components;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace TrackMyExpenses.Services
{
    public class TransactionService
    {
        private const string FilePath = @"C:\Users\Ronisha Shrestha\Desktop\22085434_Ronisha Shrestha\LocalData\transactions.json";

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
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");
            }

            // Validate user name
            if (string.IsNullOrWhiteSpace(transaction.UserName))
            {
                throw new ArgumentException("UserName must be provided for the transaction.");
            }

            // Set category if not explicitly provided
            if (string.IsNullOrEmpty(transaction.Category))
            {
                transaction.Category = transaction.Amount > 0 ? "credit" : "debit";
            }

            // Check for invalid amounts (0 or near-zero transactions)
            if (transaction.Amount == 0)
            {
                throw new InvalidOperationException("Transaction amount must not be zero.");
            }

            // Check for negative balances
            if ((transaction.Category == "debit" || transaction.TransactionType == "outflow") && transaction.Amount < 0)
            {
                decimal availableBalance = await GetAvailableBalanceForUserAsync(transaction.UserName);

                if (availableBalance < Math.Abs(transaction.Amount))
                {
                    throw new InvalidOperationException("Insufficient balance for this transaction.");
                }
            }

            // Add the transaction
            var transactions = await LoadTransactionsAsync();

            // Additional validation to avoid duplicate transactions (optional)
            bool isDuplicate = transactions.Any(t =>
                t.UserName == transaction.UserName &&
                t.Date == transaction.Date &&
                t.Amount == transaction.Amount &&
                t.Category == transaction.Category &&
                t.Description == transaction.Description);

            if (isDuplicate)
            {
                throw new InvalidOperationException("Duplicate transaction detected.");
            }

            transactions.Add(transaction);

            // Save transactions persistently
            await SaveTransactionsAsync(transactions);
        }


        public async Task<decimal> GetTotalInflowsForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "credit")
                .Sum(t => t.Amount);
        }

  
        public async Task<decimal> GetTotalOutflowsForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "debit")
                .Sum(t => Math.Abs(t.Amount));
        }
        // Get the highest inflow transaction for a specific user
        public async Task<Transactions?> GetHighestInflowForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "credit")
                .OrderByDescending(t => t.Amount)  
                .FirstOrDefault();
        }

        // Get the lowest inflow transaction for a specific user
        public async Task<Transactions?> GetLowestInflowForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "credit")
                .OrderBy(t => t.Amount) 
                .FirstOrDefault();
        }

        // Get the highest outflow transaction for a specific user
        public async Task<Transactions?> GetHighestOutflowForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "debit")
                .OrderByDescending(t => t.Amount) 
                .FirstOrDefault();
        }

        // Get the lowest outflow transaction for a specific user
        public async Task<Transactions?> GetLowestOutflowForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Category == "debit")
                .OrderBy(t => t.Amount)  
                .FirstOrDefault();
        }

       

        // Get available balance for a specific user
        public async Task<decimal> GetAvailableBalanceForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();

            // Calculate total credit and total debit for a specific user
            decimal totalCredit = transactions
                .Where(t => t.UserName == userName && t.Category == "credit")
                .Sum(t => t.Amount);
            var debtService = new DebtService();


            decimal totalDebts = await debtService.GetTotalDebtsAsync(userName);


            return totalCredit + totalDebts;
        }

        // Get total balance (credit + debit) for a specific user
        public async Task<decimal> GetTotalBalanceForUserAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();

            // Calculate total credit for a specific user
            decimal totalCredit = transactions
                .Where(t => t.UserName == userName && t.Category == "credit")
                .Sum(t => t.Amount);
            decimal totalDebit = transactions
               .Where(t => t.UserName == userName && t.Category == "debit")
               .Sum(t => t.Amount);

       
            var debtService = new DebtService();

          
            decimal totalDebts = await debtService.GetTotalDebtsAsync(userName);

         
            return totalCredit + totalDebts - totalDebit;
        }


  

  
      
        public async Task<List<Transactions>> FilterTransactionsForUserAsync(
     string userName,
     string? category = null,
     List<string>? tags = null,
     DateTime? startDate = null,
     DateTime? endDate = null,
     string? searchTitle = null, 
     bool sortByDateDescending = false 
 )
        {
            // Load all transactions asynchronously
            var transactions = await LoadTransactionsAsync();

            // Filter the transactions based on the provided criteria and userName
            var filteredTransactions = transactions
                .Where(t => t.UserName == userName) 
                                                 
                .Where(t => string.IsNullOrEmpty(category) || t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
          
                .Where(t => tags == null || !tags.Any() || tags.All(tag => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                // Filter by start date if provided
                .Where(t => !startDate.HasValue || t.Date >= startDate.Value)
                // Filter by end date if provided
                .Where(t => !endDate.HasValue || t.Date <= endDate.Value)
                // Search by title if provided
                .Where(t => string.IsNullOrEmpty(searchTitle) || t.Title.Contains(searchTitle, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Sort transactions by date if specified
            filteredTransactions = sortByDateDescending
                ? filteredTransactions.OrderByDescending(t => t.Date).ToList()
                : filteredTransactions.OrderBy(t => t.Date).ToList();

            return filteredTransactions;
        }

        // Get the top 5 most recent transactions for a specific user
        public async Task<List<Transactions>> GetTop5RecentTransactionsAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();

            // Filter the transactions for the specified user
            var userTransactions = transactions
                .Where(t => t.UserName == userName)
                .OrderByDescending(t => t.Date) 
                .Take(5) 
                .ToList();

            return userTransactions;
        }


        // Search transactions for a specific user by description
        public async Task<List<Transactions>> SearchTransactionsForUserAsync(string userName, string searchQuery)
        {
            var transactions = await LoadTransactionsAsync();
            return transactions
                .Where(t => t.UserName == userName && t.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<string> GenerateTransactionExcelAsync(string userName)
        {
            var transactions = await LoadTransactionsAsync();
            var userTransactions = transactions.Where(t => t.UserName == userName).ToList();

            var filePath = Path.Combine(Path.GetTempPath(), $"{userName}_transactions.xlsx");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transactions");

                // Add header
                worksheet.Cells[1, 1].Value = "Date";
                worksheet.Cells[1, 2].Value = "Description";
                worksheet.Cells[1, 3].Value = "Category";
                worksheet.Cells[1, 4].Value = "Amount";
                worksheet.Cells[1, 5].Value = "Tags";

                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Add transactions
                for (int i = 0; i < userTransactions.Count; i++)
                {
                    var transaction = userTransactions[i];
                    worksheet.Cells[i + 2, 1].Value = transaction.Date.ToShortDateString();
                    worksheet.Cells[i + 2, 2].Value = transaction.Description;
                    worksheet.Cells[i + 2, 3].Value = transaction.Category;
                    worksheet.Cells[i + 2, 4].Value = transaction.Amount;
                    worksheet.Cells[i + 2, 5].Value = string.Join(", ", transaction.Tags);
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                await package.SaveAsAsync(new FileInfo(filePath));
            }

            return filePath;
        }

    }
}

