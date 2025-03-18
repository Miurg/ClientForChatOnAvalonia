using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ClientForChatOnAvalonia.Interfaces.Services;
using ClientForChatOnAvalonia.Models;
using ClientForChatOnAvalonia.Services;
using ClientForChatOnAvalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClientForChatOnAvalonia
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        public override void Initialize()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
            AvaloniaXamlLoader.Load(this);
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<UsersRepository>();
            services.AddSingleton<ApiService>();
            services.AddSingleton<ChatHubService>();
            services.AddSingleton<MessageConverterService>();
            services.AddSingleton<MessagesRepository>();
            services.AddSingleton<MessagesService>();
            services.AddSingleton<SelfUserRepository>();
            services.AddSingleton<TokenService>();
            services.AddSingleton<SelfUserModel>();
            services.AddSingleton<MessangerViewModel>();
            services.AddSingleton<ChatViewModel>();
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new LoginWindow
                {
                    DataContext = new LoginViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}