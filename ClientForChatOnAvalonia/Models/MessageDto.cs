using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Models
{
    public class MessageDto
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
