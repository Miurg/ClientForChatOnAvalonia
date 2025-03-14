using Avalonia.Controls;
using ClientForChatOnAvalonia.Data;
using ClientForChatOnAvalonia.Services;

namespace ClientForChatOnAvalonia
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService = new ApiService();
        private readonly TokenService _tokenService = new TokenService();
        private readonly SelfUserDatabaseService _selfService = new SelfUserDatabaseService();

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

            var token = await _apiService.LoginAsync(username, password);

            if (token == true)
            {
                _selfService.SaveSelfUser(username);
                var messengerWindow = new MessengerWindow();
                messengerWindow.InitEvent();
                messengerWindow.Show();
                Close();
            }
            else
            {
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
