using Grocery.Core.Helpers;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IClientService _clientService;
        public AuthService(IClientService clientService)
        {
            _clientService = clientService;
        }
        public Client? Login(string email, string password)
        {
            Client? client = _clientService.Get(email);
            if (client == null) return null;
            if (PasswordHelper.VerifyPassword(password, client.Password)) return client;
            return null;
        }

        public bool Register(string name, string email, string password)
        {
            // Controleer of email al bestaat
            if (_clientService.Get(email) != null)
            {
                return false; // Email al in gebruik
            }

            // Hash het wachtwoord
            string hashedPassword = PasswordHelper.HashPassword(password);

            // Maak nieuwe client
            var newClient = new Client(0, name, email, hashedPassword);

            // Voeg toe aan database
            _clientService.Add(newClient);

            return true; // Registratie succesvol
        }
    }
}
