using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Threading;
using ClientForChatOnAvalonia.Data;
using ClientForChatOnAvalonia.Models;
using ClientForChatOnAvalonia.Services;
using ClientForChatOnAvalonia.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Avalonia.Collections;
using System.Linq;
using System.IO;
using System.Reflection;
using Avalonia.Platform;
using Avalonia;
using System.Text.RegularExpressions;

namespace ClientForChatOnAvalonia.ViewModels
{
    public class MessengerViewModel : ViewModelBase
    {
        private readonly SelfUserDatabaseService _selfUserDatabaseService;
        private readonly MessagesService _messagesService;
        private readonly MessagesDatabaseService _messagesDatabaseService;
        private readonly TokenService _tokenService;
        private readonly HubConnection _hubConnection;
        public AvaloniaList<MessageLocalModel> Messages { get; } = new();
        public ObservableCollection<EmojiItemModel> EmojiList { get; }
        private string _newMessage;
        public string NewMessage
        {
            get => _newMessage;
            set
            {
                if (SetProperty(ref _newMessage, value))
                {
                    SendMessageCommand.NotifyCanExecuteChanged();
                }
            }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }
        private bool _isEmojiPickerOpen;
        public bool IsEmojiPickerOpen
        {
            get => _isEmojiPickerOpen;
            set => SetProperty(ref _isEmojiPickerOpen, value);
        }
        public string SelectedEmoji { get; set; }

        public IAsyncRelayCommand OpenEmojiPickerCommand { get; }
        public IAsyncRelayCommand SelectEmojiCommand { get; }
        public IAsyncRelayCommand SendMessageCommand { get; }
        public IAsyncRelayCommand LoadOlderMessagesCommand { get; }
        public bool someMessegeBeingLoad = false;
        private int _loadedMessagesCount = 0;
        private const int PageSize = 20;

        public MessengerViewModel(MessagesService messagesService,
                                  SelfUserDatabaseService selfUserDatabaseService,
                                  MessagesDatabaseService messagesDatabaseService,
                                  TokenService tokenService)
        {
            _messagesService = messagesService;
            _selfUserDatabaseService = selfUserDatabaseService;
            _messagesDatabaseService = messagesDatabaseService;
            _tokenService = tokenService;

            string[] emojis = new string[3];
            string filePath = "avares://ClientForChatOnAvalonia/Assets/EmojiFile.txt";
            var uri = new Uri(filePath);

            using (var stream = AssetLoader.Open(uri))
            using (var reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();
                emojis = Regex.Matches(content, @"(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])")
                                .Cast<Match>()
                                .Select(m => m.Value)
                                .ToArray();
            }
            EmojiList = new ObservableCollection<EmojiItemModel>(
                emojis.Select(e => new EmojiItemModel(e, this))
            );

            OpenEmojiPickerCommand = new AsyncRelayCommand(async () => IsEmojiPickerOpen = !IsEmojiPickerOpen);

            SelectEmojiCommand = new AsyncRelayCommand(async () =>
            {
                if (!string.IsNullOrEmpty(SelectedEmoji))
                {
                    NewMessage += SelectedEmoji;
                    IsEmojiPickerOpen = false; 
                }
            });
            SendMessageCommand = new AsyncRelayCommand(SendMessage, () => !string.IsNullOrWhiteSpace(NewMessage));
            LoadOlderMessagesCommand = new AsyncRelayCommand(LoadOlderMessages, () => !IsLoading);

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            }; // ONLY FOR DEVELOPMENT

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://26.74.71.132:7168/messageHub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.Headers.Add("Authorization", $"Bearer {_tokenService.GetToken()}");
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<MessageModel>("ReceiveMessage", message =>
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _messagesDatabaseService.SaveMessage(message);
                    var localMessage = await _messagesService.MessageToLocalMessageAsync(message);
                    Messages.Add(localMessage);
                });
            });

            _hubConnection.On<string>("SystemMessage", message =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Messages.Add(new MessageLocalModel { Content = message, Username = "System" });
                });
            });

            _ = ConnectToHub();
        }

        private async Task ConnectToHub()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(NewMessage))
                return;

            var message = new MessageLocalModel
            {
                Content = NewMessage,
                Username = _selfUserDatabaseService.GetLastSelfUser()
            };

            var newMessage = new NewMessageModel { Content = message.Content };
            await _hubConnection.InvokeAsync("SendMessage", newMessage);
            NewMessage = string.Empty;
        }

        private async Task LoadOlderMessages()
        {
            if (IsLoading)
                return;

            IsLoading = true;

            var olderMessages = await _messagesDatabaseService.GetMessagesAsync(_loadedMessagesCount, PageSize);
            if (olderMessages.Count == 0)
            {
                IsLoading = false;
                return;
            }

            var messages = new List<MessageLocalModel>();
            foreach (var msg in olderMessages)
            {
                messages.Add(await _messagesService.MessageToLocalMessageAsync(msg));
            }

            Messages.InsertRange(0, messages);

            _loadedMessagesCount += olderMessages.Count;
            IsLoading = false;
            someMessegeBeingLoad = true;
            OnPropertyChanged(nameof(LoadOlderMessagesCommand));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}