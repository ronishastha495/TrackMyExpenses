using System.Transactions;
using TrackMyExpenses.Components.Pages;

namespace TrackMyExpenses.Models

{

    public class AppData

    {

        public List<User> Users { get; set; } = new();

        //public List<Debt> Debts { get; set; } = new();

        //public List<Transactions> Transactions { get; set; } = new(); 

    }

}