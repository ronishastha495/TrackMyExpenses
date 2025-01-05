public class Transactions
{
    public Transactions()
    {
        Tags = new List<string>(); // Initialize the Tags list
    }

    public string Id { get; set; }
    public string? UserName { get; set; }  
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; }
    public List<string> Tags { get; set; }
    public bool IsDebt { get; set; }
    public decimal? DebtAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPendingDebt { get; set; }
    public decimal? AmountPaidToClearDebt { get; set; }
    public decimal? BalanceRequired { get; set; }
    public string TransactionType { get; set; }
}
