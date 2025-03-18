using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ClientForChatOnAvalonia.ViewModels;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Threading;
using Avalonia.Controls.Presenters;
using System.Diagnostics;

namespace ClientForChatOnAvalonia;

public partial class ChatControl : UserControl
{
    private bool _saveScroll;
    private ScrollViewer _scrollViewer;
    private ListBox _listBox;
    private double currentLastScrollHeight;
    public ChatControl()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<ChatViewModel>();
        var vm = DataContext as ChatViewModel;
        vm.LoadOlderMessagesCommand.CanExecuteChanged += (_, __) => _saveScroll = true;
    }

    

    public async Task InitEvent()
    {
        _listBox = this.FindControl<ListBox>("MessagesList");
        _scrollViewer = await _listBox.GetObservable(ListBox.ScrollProperty).OfType<ScrollViewer>().FirstAsync();
        if (!(DataContext is ChatViewModel vm))
            return;

        vm.LoadOlderMessagesCommand.Execute(null);

        ((INotifyCollectionChanged)_listBox.Items).CollectionChanged += (s, args) =>
        {
            if (args.Action == NotifyCollectionChangedAction.Add && !vm.IsLoading)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _scrollViewer.Offset = new Vector(0, MaxHeight);
                    if (args.NewItems?[0] is TextBox textBox) // Проверяем, что элемент - TextBox
                    {
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
            if (DataContext is ChatViewModel vm && vm.SendMessageCommand.CanExecute(null))
            {
                vm.SendMessageCommand.Execute(null);
            }
        }
    }
    private void MessagesList_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {

        if (_scrollViewer == null)
            return;

        if (!(DataContext is ChatViewModel vm))
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