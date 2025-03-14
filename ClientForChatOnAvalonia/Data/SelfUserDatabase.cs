using Microsoft.Data.Sqlite; // Используем современный пакет
using System;
using System.IO;

namespace ClientForChatOnAvalonia.Data
{
    public class SelfUserDatabaseService
    {
        private readonly string _connectionString = "Data Source=app.db";

        public SelfUserDatabaseService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Создаем файл БД при первом подключении
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Создаем таблицу если не существует
            var createTableCmd = @"CREATE TABLE IF NOT EXISTS SelfUser (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                            Username TEXT NOT NULL)";

            using var command = new SqliteCommand(createTableCmd, connection);
            command.ExecuteNonQuery();
        }

        public void SaveSelfUser(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var insertQuery = "INSERT INTO SelfUser (Username) VALUES (@Username)";
            using (var command = new SqliteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.ExecuteNonQuery();
            }
            
        }

        public string GetLastSelfUser()
        {
            using var connection = new SqliteCommand(
                "SELECT Username FROM SelfUser ORDER BY Id DESC LIMIT 1",
                new SqliteConnection(_connectionString)
            );

            connection.Connection.Open();
            var result = connection.ExecuteScalar();
            return result?.ToString();
        }
    }
}