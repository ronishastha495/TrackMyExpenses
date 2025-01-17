using System;

namespace TrackMyExpenses.Services
{
    public class CurrencyService
    {
        private string _preferredCurrency = "USD"; // Default currency
        private decimal _exchangeRate = 1.0m; // Default exchange rate (1 USD = 1 USD)

        // Event to notify subscribers when the currency is changed
        public event Action? OnCurrencyChanged;

        // Property to get or set the preferred currency
        public string PreferredCurrency
        {
            get => _preferredCurrency;
            set
            {
                if (_preferredCurrency != value)
                {
                    _preferredCurrency = value;
                    UpdateExchangeRate();
                    NotifyCurrencyChanged();
                }
            }
        }

        // Method to convert amounts based on the current exchange rate
        public decimal ConvertAmount(decimal amount, string currency)
        {
            decimal exchangeRate = GetExchangeRate(currency);
            return Math.Round(amount * exchangeRate, 2); // Round to 2 decimal places
        }
        // Method to update the exchange rate based on the selected currency
        private void UpdateExchangeRate()
        {
            _exchangeRate = GetExchangeRate(_preferredCurrency);
        }

        // Method to fetch the exchange rate for a given currency
        private decimal GetExchangeRate(string currency)
        {
            return currency switch
            {
                "NRS" => 130.0m, // Example: 1 USD = 130 NRS
                "USD" => 1.0m,   // Base currency (USD)
                _ => 1.0m        // Default to USD if unknown
            };
        }

        // Notify subscribers about the currency change
        private void NotifyCurrencyChanged()
        {
            OnCurrencyChanged?.Invoke();
        }
    }
}
