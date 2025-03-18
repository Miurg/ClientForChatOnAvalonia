using ClientForChatOnAvalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Models
{
    public class EmojiItemModel
    {
        public string Emoji { get; set; }

        public EmojiItemModel(string emoji)
        {
            Emoji = emoji;
        }
    }
}
