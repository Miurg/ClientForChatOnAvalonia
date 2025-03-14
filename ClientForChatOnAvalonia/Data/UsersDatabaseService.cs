using ClientForChatOnAvalonia.Models;
using ClientForChatOnAvalonia.Services;
using Microsoft.Data.Sqlite; // Updated SQLite package
using System;
using System.IO;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Data
{
    public class UsersDatabaseService
    {
        private readonly string _connectionString = "Data Source=app.db";
        private readonly ApiService _apiService;

        public UsersDatabaseService(ApiService apiService)
        {
            // Создаем файл БД, если его нет (через FileStream)
            if (!File.Exists("app.db"))
            {
                using (File.Create("app.db")) { }
            }

            InitializeDatabase();
            _apiService = apiService;
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Создаем таблицу Users, если она не существует
            var createTableCmd = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY, 
                    Username TEXT NOT NULL
                );";

            using var command = new SqliteCommand(createTableCmd, connection);
            command.ExecuteNonQuery();
        }

        public async Task<UserModel> GetOrFetchUser(int userId)
        {
            var user = GetUserById(userId);
            if (user == null)
            {
                user = await _apiService.FetchUserAsync(userId);
                if (user != null)
                {
                    AddUser(user);
                }
            }
            return user;
        }

        private UserModel GetUserById(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT Id, Username FROM Users WHERE Id = @UserId";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new UserModel
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1)
                };
            }

            return null;
        }

        private void AddUser(UserModel user)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "INSERT INTO Users (Id, Username) VALUES (@UserId, @Username)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", user.Id);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.ExecuteNonQuery();
        }
    }
}