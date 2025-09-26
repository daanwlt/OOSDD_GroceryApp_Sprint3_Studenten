using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grocery.Core.Services;
using Grocery.Core.Data.Repositories;
using Grocery.Core.Models;
using Grocery.Core.Helpers;

namespace TestCore
{
    [TestClass]
    public class UC09RegistrationTests
    {
        private AuthService _authService;
        private ClientRepository _clientRepository;
        private ClientService _clientService;

        [TestInitialize]
        public void Setup()
        {
            _clientRepository = new ClientRepository();
            _clientService = new ClientService(_clientRepository);
            _authService = new AuthService(_clientService);
        }

        [TestMethod]
        [TestCategory("UC09")]
        [TestName("HappyPath")]
        public void UC09_HappyPath_SuccessfulRegistration()
        {
            // Arrange
            string name = "Test Gebruiker";
            string email = "test.gebruiker@example.com";
            string password = "TestWachtwoord123";

            // Act
            bool result = _authService.Register(name, email, password);

            // Assert
            Assert.IsTrue(result, "Registratie zou succesvol moeten zijn");
            
            // Verify user was created
            var createdUser = _clientService.Get(email);
            Assert.IsNotNull(createdUser, "Gebruiker zou aangemaakt moeten zijn");
            Assert.AreEqual(name, createdUser.Name, "Naam zou correct moeten zijn");
            Assert.AreEqual(email, createdUser.EmailAddress, "Email zou correct moeten zijn");
            
            // Verify password is hashed
            Assert.AreNotEqual(password, createdUser.Password, "Wachtwoord zou gehashed moeten zijn");
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, createdUser.Password), "Wachtwoord verificatie zou moeten werken");
        }

        [TestMethod]
        [TestCategory("UC09")]
        [TestName("UnhappyPath")]
        public void UC09_UnhappyPath_DuplicateEmail()
        {
            // Arrange
            string name1 = "Eerste Gebruiker";
            string name2 = "Tweede Gebruiker";
            string email = "duplicate@example.com";
            string password = "TestWachtwoord123";

            // Act - First registration should succeed
            bool firstResult = _authService.Register(name1, email, password);
            Assert.IsTrue(firstResult, "Eerste registratie zou succesvol moeten zijn");

            // Act - Second registration with same email should fail
            bool secondResult = _authService.Register(name2, email, password);

            // Assert
            Assert.IsFalse(secondResult, "Tweede registratie met zelfde email zou moeten falen");
            
            // Verify only one user exists
            var user = _clientService.Get(email);
            Assert.IsNotNull(user, "Gebruiker zou moeten bestaan");
            Assert.AreEqual(name1, user.Name, "Eerste gebruiker naam zou behouden moeten blijven");
        }

        [TestMethod]
        [TestCategory("UC09")]
        [TestName("Validation")]
        public void UC09_Validation_InputValidation()
        {
            // Test empty name
            Assert.IsFalse(_authService.Register("", "test@example.com", "password123"), 
                "Lege naam zou moeten falen");

            // Test short name
            Assert.IsFalse(_authService.Register("A", "test@example.com", "password123"), 
                "Naam met 1 karakter zou moeten falen");

            // Test invalid email
            Assert.IsFalse(_authService.Register("Test User", "invalid-email", "password123"), 
                "Ongeldig email format zou moeten falen");

            // Test short password
            Assert.IsFalse(_authService.Register("Test User", "test@example.com", "123"), 
                "Wachtwoord met minder dan 6 karakters zou moeten falen");

            // Test valid registration
            Assert.IsTrue(_authService.Register("Valid User", "valid@example.com", "validpassword123"), 
                "Geldige registratie zou moeten slagen");
        }

        [TestMethod]
        [TestCategory("UC09")]
        [TestName("PasswordSecurity")]
        public void UC09_PasswordSecurity_Hashing()
        {
            // Arrange
            string password = "TestPassword123";
            string hashedPassword = PasswordHelper.HashPassword(password);

            // Assert
            Assert.AreNotEqual(password, hashedPassword, "Wachtwoord zou gehashed moeten zijn");
            Assert.IsTrue(hashedPassword.Length > 50, "Gehashed wachtwoord zou lang moeten zijn");
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, hashedPassword), 
                "Wachtwoord verificatie zou moeten werken");
            Assert.IsFalse(PasswordHelper.VerifyPassword("wrongpassword", hashedPassword), 
                "Verkeerd wachtwoord zou moeten falen");
        }

        [TestMethod]
        [TestCategory("UC09")]
        [TestName("Navigation")]
        public void UC09_Navigation_RegistrationFlow()
        {
            // This test would typically test UI navigation
            // For now, we'll test the business logic that supports navigation
            
            // Arrange
            string name = "Navigation Test";
            string email = "navigation@example.com";
            string password = "NavigationTest123";

            // Act
            bool registrationResult = _authService.Register(name, email, password);

            // Assert
            Assert.IsTrue(registrationResult, "Registratie zou succesvol moeten zijn voor navigatie test");
            
            // Verify user can be retrieved (simulating successful login after registration)
            var user = _clientService.Get(email);
            Assert.IsNotNull(user, "Gebruiker zou beschikbaar moeten zijn na registratie");
        }
    }
}
