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
        private readonly MessengerViewModel _viewModel;

        public EmojiItemModel(string emoji, MessengerViewModel viewModel)
        {
            Emoji = emoji;
            _viewModel = viewModel;
        }

        public void OnClick()
        {
            _viewModel.NewMessage += Emoji;
        }
    }
}
