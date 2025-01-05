using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TrackMyExpenses.Models;

public class CashInflowService
{
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private static readonly string FolderPath = Path.Combine(DesktopPath, "LocalDB");
    private static readonly string FilePath = Path.Combine(FolderPath, "cashInflows.json");

    private int _idCounter = 1;

    // Load cash inflows from the file
    public async Task<List<CashInflowModel>> LoadCashInflowsAsync()
    {
        if (!File.Exists(FilePath))
            return new List<CashInflowModel>();  // Return an empty list if no data exists

        var json = await File.ReadAllTextAsync(FilePath);
        return JsonSerializer.Deserialize<List<CashInflowModel>>(json) ?? new List<CashInflowModel>();
    }

    // Save cash inflows to the file
    public async Task SaveCashInflowsAsync(List<CashInflowModel> inflows)
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        if (!File.Exists(FilePath))
        {
            File.WriteAllText(FilePath, "[]");  // Initialize with empty array if no file exists
        }

        var json = JsonSerializer.Serialize(inflows, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(FilePath, json);
    }

    // Add a new cash inflow
    public async Task AddCashInflowAsync(CashInflowModel inflow)
    {
        var inflows = await LoadCashInflowsAsync();
        inflow.Id = _idCounter++;
        inflows.Add(inflow);
        await SaveCashInflowsAsync(inflows);
    }

    // Get all cash inflows
    public async Task<List<CashInflowModel>> GetAllCashInflowsAsync()
    {
        return await LoadCashInflowsAsync();
    }

    // Get the total cash inflow
    public async Task<decimal> GetTotalCashInflowAsync()
    {
        var inflows = await LoadCashInflowsAsync();
        return inflows.Sum(x => x.Amount);
    }
}
