using ClientForChatOnAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Services
{
    public class MessageConverterService
    {
        private readonly SelfUserModel _selfUserModel;
        private readonly UsersRepository _usersRepository;
        public MessageConverterService(SelfUserModel selfUserModel, UsersRepository usersRepository) 
        {
            _selfUserModel = selfUserModel;
            _usersRepository = usersRepository;
        }
        public async Task<MessageModel> MessageDtoToMessageModelAsync(MessageDto message)
        {
            var username = await _usersRepository.GetOrFetchUser(message.UserID);
            Debug.WriteLine(username.Username + " " + _selfUserModel.UserName);
            return new MessageModel
            {
                Username = username.Username,
                Content = message.Content,
                IsFromCurrentUser = username.Username == _selfUserModel.UserName,
                CreatedAt = message.CreatedAt,
            };
        }
    }
}
