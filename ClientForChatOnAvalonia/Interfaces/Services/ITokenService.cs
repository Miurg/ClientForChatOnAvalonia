using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Interfaces.Services
{
    public interface ITokenService
    {
        public void SaveToken(string token);
        public string GetToken();
        public void DeleteToken();
    }
}
