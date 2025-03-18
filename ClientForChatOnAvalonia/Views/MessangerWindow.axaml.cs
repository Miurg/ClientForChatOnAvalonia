using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ClientForChatOnAvalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System;
using ClientForChatOnAvalonia.Models;

namespace ClientForChatOnAvalonia;

public partial class MessangerWindow : Window
{
    public MessangerWindow(string username)
    {
        InitializeComponent();
        var user = App.Services.GetRequiredService<SelfUserModel>();
        user.UserName = username;
        DataContext = App.Services.GetRequiredService<MessangerViewModel>();
    }
    
}