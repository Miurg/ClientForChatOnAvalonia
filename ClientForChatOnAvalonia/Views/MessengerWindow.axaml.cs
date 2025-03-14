using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ClientForChatOnAvalonia.Data;
using ClientForChatOnAvalonia.Models;
using ClientForChatOnAvalonia.Services;
using ClientForChatOnAvalonia.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia
{
    public partial class MessengerWindow : Window
    {
        private bool _saveScroll;
        private ScrollViewer _scrollViewer;
        private ListBox _listBox;
        private double currentLastScrollHeight;
        public MessengerWindow()
        {
            InitializeComponent();
            var apiService = new ApiService();
            var usersService = new UsersDatabaseService(apiService);
            var selfUserService = new SelfUserDatabaseService();
            var messagesService = new MessagesService(usersService, selfUserService);
            var messagesDatabase = new MessagesDatabaseService(apiService, usersService);
            var tokenService = new TokenService();

            DataContext = new MessengerViewModel(messagesService, selfUserService, messagesDatabase, tokenService);
            this.AttachDevTools();

            var vm = DataContext as MessengerViewModel; 
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MessengerViewModel.LoadOlderMessagesCommand))
                {
                    _saveScroll = true;
                }
            };
        }
        public async Task InitEvent()
        {
            _listBox = this.FindControl<ListBox>("MessagesList");
            _scrollViewer = await _listBox.GetObservable(ListBox.ScrollProperty).OfType<ScrollViewer>().FirstAsync();
            if (!(DataContext is MessengerViewModel vm))
                return;

            vm.LoadOlderMessagesCommand.Execute(null);

            ((INotifyCollectionChanged)_listBox.Items).CollectionChanged += (s, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add && !vm.IsLoading)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _scrollViewer.Offset = new Vector(0, MaxHeight);
                        Debug.WriteLine(args.NewItems?[0]);
                        if (args.NewItems?[0] is TextBox textBox) // Проверяем, что элемент - TextBox
                        {
                            Debug.WriteLine(textBox.Text);
                            textBox.TemplateApplied += (_, args) =>
                            {
                                if (textBox.FindControl<TextPresenter>("PART_TextPresenter") is { } textPresenter)
                                {
                                    textPresenter.Width = 200;
                                    textPresenter.Height = 10;
                                }
                            };
                        }
                        
                    }, DispatcherPriority.Background);
                    
                }
            };
            _scrollViewer.GetObservable(ScrollViewer.ScrollBarMaximumProperty).Subscribe((Action<Vector>)(i => SaveScrollPosition(i)));
        }
        public void SaveScrollPosition(Vector maxHeight)
        {
            if (_saveScroll == false)
                return;

            _scrollViewer.Offset = new Vector(0, maxHeight.Y - currentLastScrollHeight);

            _saveScroll = false;
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is MessengerViewModel vm && vm.SendMessageCommand.CanExecute(null))
                {
                    vm.SendMessageCommand.Execute(null);
                }
            }
        }
        private void MessagesList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollViewer == null)
                return;

            if (!(DataContext is MessengerViewModel vm))
                return;

            if (!vm.LoadOlderMessagesCommand.CanExecute(null) || vm.IsLoading)
                return;

            if (_scrollViewer.Offset.Y == 0)
            {
               
                currentLastScrollHeight = _scrollViewer.ScrollBarMaximum.Y;
                vm.LoadOlderMessagesCommand.Execute(null);
            }
        }
    }
}
