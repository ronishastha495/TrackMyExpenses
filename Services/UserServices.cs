using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using TrackMyExpenses.Models;

namespace BudgetEase.Services
{
    public class UserServices
    {
        private const string FilePath = @"C:\Users\Ronisha Shrestha\Desktop\TrackMyExpenses\LocalDB\users.json";

        private static User? _loggedInUser;
        public List<User> LoadUsers()
        {
            if (!File.Exists(FilePath))
            {

                return new List<User>();
            }

            var json = File.ReadAllText(FilePath);


            if (string.IsNullOrWhiteSpace(json))
            {

                return new List<User>();
            }

            try
            {
                var users = JsonSerializer.Deserialize<List<User>>(json);
                return users ?? new List<User>();
            }
            catch (JsonException ex)
            {

                Console.WriteLine($"JSON deserialization failed: {ex.Message}");
                return new List<User>();
            }
        }


        public void SaveUsers(List<User> users)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Serialize and save user data
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }


        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);  // Return hashed password
        }
        public void SetLoggedInUser(User user)
        {
            _loggedInUser = user;
        }

        public User? GetLoggedInUser()
        {
            return _loggedInUser;
        }
        public bool ValidatePassword(string inputPassword, string storedPassword)
        {
            var hashedInputPassword = HashPassword(inputPassword);
            return hashedInputPassword == storedPassword;
        }

        public string GetPreferredCurrency()
        {
            return _loggedInUser?.PreferredCurrency ?? "USD";
        }
    }
}
