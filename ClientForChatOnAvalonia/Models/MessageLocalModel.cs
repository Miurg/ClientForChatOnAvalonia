using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Models
{
    public class MessageLocalModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFromCurrentUser { get; set; }
    }
}
