using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClientForChatOnAvalonia.ViewModels
{
    public partial class MessangerViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object _currentView;

        public MessangerViewModel()
        {
            ChatControl chat = new ChatControl();
            chat.InitEvent();
            CurrentView = chat;
        }
    }
}
