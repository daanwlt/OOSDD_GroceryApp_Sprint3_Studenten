
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string email = "user3@mail.com";

        [ObservableProperty]
        private string password = "user3";

        [ObservableProperty]
        private string loginMessage;

        public LoginViewModel(IAuthService authService, GlobalViewModel global, IServiceProvider serviceProvider)
        { //_authService = App.Services.GetServices<IAuthService>().FirstOrDefault();
            _authService = authService;
            _global = global;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private void Login()
        {
            Client? authenticatedClient = _authService.Login(Email, Password);
            if (authenticatedClient != null)
            {
                LoginMessage = $"Welkom {authenticatedClient.Name}!";
                _global.Client = authenticatedClient;
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                LoginMessage = "Ongeldige inloggegevens.";
            }
        }

        [RelayCommand]
        private async Task GoToRegister()
        {
            try
            {
                // Navigeer naar RegisterView via service provider
                var registerViewModel = _serviceProvider.GetService<RegisterViewModel>();
                if (registerViewModel != null)
                {
                    Application.Current.MainPage = new RegisterView(registerViewModel);
                }
                else
                {
                    LoginMessage = "Registratie service niet beschikbaar.";
                }
            }
            catch (Exception ex)
            {
                LoginMessage = $"Navigatie fout: {ex.Message}";
            }
        }
    }
}
