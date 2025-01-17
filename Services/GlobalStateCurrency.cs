using System;
using TrackMyExpenses.Model; // Adjust namespace if necessary

namespace TrackMyExpenses.Services
{
    public class GlobalState
    {
        private static GlobalState _instance;
        private readonly UserServices _userServices; // Reference to UserServices

        private GlobalState()
        {
            _userServices = new UserServices(); // Initialize UserServices
        }

        public static GlobalState Instance => _instance ??= new GlobalState();

        public string PreferredCurrency
        {
            get => _userServices.GetPreferredCurrency(); // Fetch currency from UserServices
            set
            {
                if (_userServices.GetPreferredCurrency() != value)
                {
                    var user = _userServices.GetLoggedInUser();
                    if (user != null)
                    {
                        user.PreferredCurrency = value; // Update user preference
                        _userServices.SaveUsers(_userServices.LoadUsers()); // Persist changes
                        NotifyStateChanged();
                    }
                }
            }
        }

        public User? CurrentUser
        {
            get => _userServices.GetLoggedInUser(); // Fetch current user from UserServices
            set
            {
                if (_userServices.GetLoggedInUser() != value)
                {
                    _userServices.SetLoggedInUser(value); // Update the logged-in user
                    NotifyStateChanged();
                }
            }
        }

        public event Action? StateChanged;

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
