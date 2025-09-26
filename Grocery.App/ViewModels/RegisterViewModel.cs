using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using System.Text.RegularExpressions;

namespace Grocery.App.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string successMessage = string.Empty;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private bool hasSuccess = false;

        public RegisterViewModel(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task Register()
        {
            // Reset meldingen
            HasError = false;
            HasSuccess = false;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Validatie
            if (!ValidateInput())
            {
                return;
            }

            // Probeer te registreren
            bool registrationSuccess = _authService.Register(Name, Email, Password);

            if (registrationSuccess)
            {
                SuccessMessage = "Account succesvol aangemaakt! U kunt nu inloggen.";
                HasSuccess = true;
                
                // Wacht 2 seconden en ga dan naar login
                await Task.Delay(2000);
                await GoToLogin();
            }
            else
            {
                ErrorMessage = "Dit email adres is al in gebruik. Probeer een ander email adres.";
                HasError = true;
            }
        }

        [RelayCommand]
        private async Task GoToLogin()
        {
            try
            {
                // Navigeer terug naar LoginView
                var loginViewModel = _serviceProvider.GetService<LoginViewModel>();
                if (loginViewModel != null)
                {
                    Application.Current.MainPage = new LoginView(loginViewModel);
                }
                else
                {
                    ErrorMessage = "Login service niet beschikbaar.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Navigatie fout: {ex.Message}";
                HasError = true;
            }
        }

        private bool ValidateInput()
        {
            // Controleer naam
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Naam is verplicht.";
                HasError = true;
                return false;
            }

            if (Name.Length < 2)
            {
                ErrorMessage = "Naam moet minimaal 2 karakters bevatten.";
                HasError = true;
                return false;
            }

            // Controleer email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email adres is verplicht.";
                HasError = true;
                return false;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Voer een geldig email adres in.";
                HasError = true;
                return false;
            }

            // Controleer wachtwoord
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Wachtwoord is verplicht.";
                HasError = true;
                return false;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Wachtwoord moet minimaal 6 karakters bevatten.";
                HasError = true;
                return false;
            }

            // Controleer wachtwoord bevestiging
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Wachtwoord bevestiging is verplicht.";
                HasError = true;
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Wachtwoorden komen niet overeen.";
                HasError = true;
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
