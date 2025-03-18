using Avalonia.Animation;
using ClientForChatOnAvalonia.Interfaces.Services;
using ClientForChatOnAvalonia.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly UsersRepository _usersRepository;
        private readonly SelfUserModel _selfUserModel;
        private readonly MessagesRepository _messagesRepository;
        private readonly ChatHubService _chatHubService;
        private readonly MessageConverterService _messageConverterService;

        public MessagesService(UsersRepository usersRepository, 
            SelfUserModel selfUserModel, 
            MessagesRepository messagesRepository, 
            ChatHubService chatHubService,
            MessageConverterService messageConverterService)
        {
            _usersRepository = usersRepository;
            _selfUserModel = selfUserModel;
            _messagesRepository = messagesRepository;
            _chatHubService = chatHubService;
            _messageConverterService = messageConverterService;
        }
        public async Task SendMessageAsync(string content)
        {
            await _chatHubService.SendMessageAsync(content);
        }
        public async Task<List<MessageModel>> LoadOlderMessages(int LoadedMessagesCount,int PageSize)
        {
            var olderMessages = await _messagesRepository.GetMessagesAsync(LoadedMessagesCount, PageSize);
            if (olderMessages.Count == 0)
            {
                return null;
            }

            var messages = new List<MessageModel>();
            foreach (var msg in olderMessages)
            {
                messages.Add(await _messageConverterService.MessageDtoToMessageModelAsync(msg));
            }
            return messages;
        }
    }
}
