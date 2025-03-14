using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ClientForChatOnAvalonia.Models;
using ClientForChatOnAvalonia.Services;
using Microsoft.Data.Sqlite;
using Tmds.DBus.Protocol;

namespace ClientForChatOnAvalonia.Data
{
    public class MessagesDatabaseService
    {
        private readonly string _connectionString = "Data Source=app.db";

        private readonly ApiService _apiService;
        private readonly UsersDatabaseService _usersDatabaseService;

        public MessagesDatabaseService(ApiService apiService, UsersDatabaseService usersDatabaseService)
        {
             InitializeDatabase();
            _apiService = apiService;
            _usersDatabaseService = usersDatabaseService;
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY,
                    Content TEXT NOT NULL,
                    UserID INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (UserID) REFERENCES Users(Id)
                )";
                    using var command = new SqliteCommand(createTableQuery, connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                
            }
        }

        public async Task SaveMessage(MessageModel message)
        {
            await _usersDatabaseService.GetOrFetchUser(message.UserID);
            using (var connection = new SqliteConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string insertMessage = @"
                INSERT INTO Messages (Id, Content, UserID, CreatedAt)
                VALUES (@Id, @Content, @UserID, @CreatedAt)
                ON CONFLICT(Id) DO NOTHING;";

                    using (var command = new SqliteCommand(insertMessage, connection))
                    {
                        command.Parameters.AddWithValue("@Id", message.Id);
                        command.Parameters.AddWithValue("@Content", message.Content);
                        command.Parameters.AddWithValue("@UserID", message.UserID);
                        command.Parameters.AddWithValue("@CreatedAt", message.CreatedAt.ToString("o"));
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        public async Task<List<MessageModel>> GetMessagesAsync(int offset, int limit)
        {

            var messagesToFetch = await _apiService.FetchMessagesAsync(offset, limit);
            if (messagesToFetch != null)
            {
                await SaveMessages(messagesToFetch);
            }
            var messages = new List<MessageModel>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                string selectMessages = @"
                SELECT Id, Content, UserID, CreatedAt 
                FROM Messages 
                ORDER BY CreatedAt DESC
                LIMIT @Limit OFFSET @Offset";

                using (var command = new SqliteCommand(selectMessages, connection))
                {
                    command.Parameters.AddWithValue("@Limit", limit);
                    command.Parameters.AddWithValue("@Offset", offset);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new MessageModel
                            {
                                Id = reader.GetInt32(0),
                                Content = reader.GetString(1),
                                UserID = reader.GetInt32(2),
                                CreatedAt = DateTime.Parse(reader.GetString(3))
                            });
                        }
                    }
                }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            messages.Reverse();
            return messages;
        }

        public async Task SaveMessages(List<MessageModel> messages)
        {
            foreach (var message in messages)
                await _usersDatabaseService.GetOrFetchUser(message.UserID);
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    string insertMessage = @"
            INSERT INTO Messages (Id, Content, UserID, CreatedAt)
            VALUES (@Id, @Content, @UserID, @CreatedAt)
            ON CONFLICT(Id) DO NOTHING;";

                    using (var command = new SqliteCommand(insertMessage, connection, transaction))
                    {
                        command.Parameters.Add("@Id", SqliteType.Text);
                        command.Parameters.Add("@Content", SqliteType.Text);
                        command.Parameters.Add("@UserID", SqliteType.Integer);
                        command.Parameters.Add("@CreatedAt", SqliteType.Text);

                        foreach (var message in messages)
                        {
                            command.Parameters["@Id"].Value = message.Id;
                            command.Parameters["@Content"].Value = message.Content;
                            command.Parameters["@UserID"].Value = message.UserID;
                            command.Parameters["@CreatedAt"].Value = message.CreatedAt.ToString("o");

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

    }
}
