using Avalonia.Controls;
using ClientForChatOnAvalonia.Services;
using ClientForChatOnAvalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClientForChatOnAvalonia
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var username = LoginField.Text;
            var password = PasswordField.Text;

            if (string.IsNullOrEmpty(username))
            {
                ErrorLabel.IsVisible = true;
                return;
            }
            var apiService = App.Services.GetRequiredService<ApiService>();
            var token = await apiService.LoginAsync(username, password);

            if (token == true)
            {
                var messangerWindow = new MessangerWindow(username);
                messangerWindow.Show();
                Close();
            }
            else
            {
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
