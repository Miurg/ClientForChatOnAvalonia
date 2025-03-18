using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public required string Username { get; set; }
    }
}
