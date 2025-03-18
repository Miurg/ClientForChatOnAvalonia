using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public required string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFromCurrentUser { get; set; }

        

    }
}
