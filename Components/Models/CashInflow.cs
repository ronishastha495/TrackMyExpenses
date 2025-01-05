public class CashInflowModel
{
    public string Title { get; set; }
    public string Label { get; set; }
    public decimal Amount { get; set; }
    public string Source { get; set; }
    public DateTime Date { get; set; }
    public string Note { get; set; }
    public int Id { get; internal set; }
}
