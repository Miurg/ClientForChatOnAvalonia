using ClientForChatOnAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Interfaces.Services
{
    public interface IChatHubService
    {
        public event Func<MessageModel, Task>? OnMessageReceived;

        Task ConnectAsync();
        Task SendMessageAsync(string content);
    }
}
