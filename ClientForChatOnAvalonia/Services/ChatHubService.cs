using Avalonia.Threading;
using ClientForChatOnAvalonia.Interfaces.Services;
using ClientForChatOnAvalonia.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Services
{
    public class ChatHubService : IChatHubService
    {
        private readonly HubConnection _hubConnection;
        private readonly MessageConverterService _messageConverterService;
        private readonly MessagesRepository _messagesRepository;

        public event Func<MessageModel, Task>? OnMessageReceived;

        public ChatHubService(MessageConverterService messageConverterService,
                              MessagesRepository messagesRepository,
                              TokenService tokenService)
        {
            _messagesRepository = messagesRepository;
            _messageConverterService = messageConverterService;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            }; // ONLY FOR DEVELOPMENT

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://26.74.71.132:7168/messageHub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.Headers.Add("Authorization", $"Bearer {tokenService.GetToken()}");
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<MessageDto>("ReceiveMessage", async message =>
            {
                await _messagesRepository.SaveMessage(message);
                var localMessage = await _messageConverterService.MessageDtoToMessageModelAsync(message);

                if (OnMessageReceived != null)
                {
                    var handlers = OnMessageReceived.GetInvocationList()
                        .Cast<Func<MessageModel, Task>>();

                    foreach (var handler in handlers)
                    {
                        await handler(localMessage);
                    }
                }
            });
            _ = ConnectAsync();
        }

        public async Task ConnectAsync()
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

        public async Task SendMessageAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            var newMessage = new NewMessageModel { Content = content };
            await _hubConnection.InvokeAsync("SendMessage", newMessage);
        }
    }
}
