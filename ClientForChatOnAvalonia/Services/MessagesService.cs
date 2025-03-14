using ClientForChatOnAvalonia.Data;
using ClientForChatOnAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Services
{
    public class MessagesService
    {
        private readonly UsersDatabaseService _usersDatabaseService;
        private readonly SelfUserDatabaseService _selfuserDatabaseService;

        public MessagesService(UsersDatabaseService usersDatabaseService, SelfUserDatabaseService selfuserDatabaseService)
        {
            _usersDatabaseService = usersDatabaseService;
            _selfuserDatabaseService = selfuserDatabaseService;
        }

        public async Task<MessageLocalModel> MessageToLocalMessageAsync(MessageModel message)
        {
            var username = await _usersDatabaseService.GetOrFetchUser(message.UserID);
            return new MessageLocalModel
            {
                Username = username.Username,
                Content = message.Content,
                IsFromCurrentUser = username.Username == _selfuserDatabaseService.GetLastSelfUser(),
                CreatedAt = message.CreatedAt,
            };
        }
    }
}
