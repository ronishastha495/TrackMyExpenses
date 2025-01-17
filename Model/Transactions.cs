public class Transactions
{
    public Transactions()
    {
        Tags = new List<string>(); 
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
   
    public string TransactionType { get; set; }
}
