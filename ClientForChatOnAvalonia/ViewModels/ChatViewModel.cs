using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Platform;
using ClientForChatOnAvalonia.Models;
using ClientForChatOnAvalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientForChatOnAvalonia.ViewModels
{
    public partial class ChatViewModel : ViewModelBase
    {
        private readonly SelfUserRepository _selfUserDatabaseService;
        private readonly MessagesService _messagesService;
        private readonly MessagesRepository _messagesRepository;
        private readonly TokenService _tokenService;
        private readonly ChatHubService _chatHubService;
        public AvaloniaList<MessageModel> Messages { get; } = new();
        public AvaloniaList<EmojiItemModel> EmojiList { get; }

        [ObservableProperty]
        private string _newMessage;
        [ObservableProperty]
        private bool _isEmojiPickerOpen;
        public string SelectedEmoji { get; set; }
        [ObservableProperty]
        private bool _isLoading;

        private int _loadedMessagesCount = 0;
        private const int DefaultPageSize = 20;
        public IAsyncRelayCommand OpenEmojiPickerCommand { get; }
        public IAsyncRelayCommand SendMessageCommand { get; }
        public IAsyncRelayCommand LoadOlderMessagesCommand { get; }
        public IAsyncRelayCommand OnEmojiClick {  get; }
        public ChatViewModel(SelfUserRepository selfUserRepository,
            MessagesService messagesService,
            MessagesRepository messagesRepository,
            TokenService tokenService,
            ChatHubService chatHubService)
        {
            _messagesService = messagesService;
            _messagesRepository = messagesRepository;
            _tokenService = tokenService;
            _selfUserDatabaseService = selfUserRepository;
            _chatHubService = chatHubService;

            string[] emojis = InitEmojiList();
            EmojiList = new AvaloniaList<EmojiItemModel>(
                emojis.Select(e => new EmojiItemModel(e))
            );

            OnEmojiClick = new AsyncRelayCommand<object>(ExecuteOnEmojiClick);
            OpenEmojiPickerCommand = new AsyncRelayCommand(async () => IsEmojiPickerOpen = !IsEmojiPickerOpen);
            SendMessageCommand = new AsyncRelayCommand(SendMessage, () => !string.IsNullOrWhiteSpace(NewMessage));
            LoadOlderMessagesCommand = new AsyncRelayCommand(LoadOlderMessages, () => !IsLoading);
            _chatHubService.OnMessageReceived += async (message) =>
            {
                try
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => Messages.Add(message));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error handling received message: {ex}");
                }
            };
        }

        private string[] InitEmojiList()
        {
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
            return emojis;
        }

        private async Task ExecuteOnEmojiClick(object? emoji)
        {
            if (emoji is string emojiStr)
            {
                NewMessage += emojiStr;
            }
        }
        private async Task SendMessage()
        {
            try
            {
                await _messagesService.SendMessageAsync(NewMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
            NewMessage = string.Empty;
        }
        public async Task LoadOlderMessages()
        {
            if (IsLoading)
                return;
            IsLoading = true;
            
            try
            {
                List<MessageModel> messages = await _messagesService.LoadOlderMessages(_loadedMessagesCount, DefaultPageSize);
                if (messages == null)
                {
                    IsLoading = false;
                    return;
                }
                Messages.InsertRange(0, messages);

                _loadedMessagesCount += messages.Count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
            IsLoading = false;

        }
        partial void OnNewMessageChanged(string value)
        {
            SendMessageCommand.NotifyCanExecuteChanged();
        }
    }
}
