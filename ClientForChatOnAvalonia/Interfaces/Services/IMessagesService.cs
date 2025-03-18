using ClientForChatOnAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Interfaces.Services
{
    public interface IMessagesService
    {
        Task SendMessageAsync(string content);
    }
}
