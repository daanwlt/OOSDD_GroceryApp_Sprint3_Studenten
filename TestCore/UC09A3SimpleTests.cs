using Xunit;
using Moq;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System;

namespace TestCore
{
    /// <summary>
    /// UC09 Registratie Gebruiker - Eenvoudige A3 Tests
    /// 
    /// Deze tests demonstreren de A3 methode op de Core business logic:
    /// - Arrange: Setup van test data en service mocks
    /// - Act: Uitvoering van service methoden
    /// - Assert: Verificatie van verwachte resultaten
    /// 
    /// Best Practices:
    /// - Modulair: Separate test methods per functionaliteit
    /// - Herbruikbaar: Test data constants
    /// - Onderhoudbaar: Duidelijke naming en documentation
    /// </summary>
    [Trait("Category", "UC09")]
    [Trait("TestType", "A3_Method_Simple")]
    public class UC09A3SimpleTests : IDisposable
    {
        #region Test Constants

        private const string ValidName = "Test Gebruiker";
        private const string ValidEmail = "test.gebruiker@example.com";
        private const string ValidPassword = "TestWachtwoord123";
        private const string ExistingEmail = "bestaand@example.com";

        #endregion

        #region Test Infrastructure

        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IClientService> _mockClientService;

        public UC09A3SimpleTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockClientService = new Mock<IClientService>();
        }

        public void Dispose()
        {
            _mockAuthService?.Reset();
            _mockClientService?.Reset();
        }

        #endregion

        #region A3 Test Methods - AuthService Tests

        /// <summary>
        /// A3 Test: Succesvolle Registratie via AuthService
        /// 
        /// Arrange: Valid user data, successful client service mock
        /// Act: Execute registration via AuthService
        /// Assert: Verify successful registration and service calls
        /// </summary>
        [Fact(DisplayName = "A3 Simple - Happy Path: Succesvolle registratie via AuthService")]
        [Trait("TestName", "AuthService_SuccessfulRegistration")]
        public void A3_Simple_AuthService_SuccessfulRegistration()
        {
            // ===== ARRANGE =====
            // Setup client service to return null (email not exists)
            _mockClientService.Setup(s => s.Get(ValidEmail))
                             .Returns((Client)null);
            
            // Setup client service to return new client when added
            var newClient = new Client(1, ValidName, ValidEmail, "hashedpassword");
            _mockClientService.Setup(s => s.Add(It.IsAny<Client>()))
                             .Returns(newClient);

            // Create AuthService with mocked dependencies
            var authService = new Grocery.Core.Services.AuthService(_mockClientService.Object);

            // ===== ACT =====
            var result = authService.Register(ValidName, ValidEmail, ValidPassword);

            // ===== ASSERT =====
            Assert.True(result, "Registration should be successful");
            
            // Verify client service was called to check email existence
            _mockClientService.Verify(s => s.Get(ValidEmail), Times.Once, 
                "ClientService.Get should be called to check email existence");
            
            // Verify client service was called to add new client
            _mockClientService.Verify(s => s.Add(It.Is<Client>(c => 
                c.Name == ValidName && 
                c.EmailAddress == ValidEmail && 
                !string.IsNullOrEmpty(c.Password))), Times.Once, 
                "ClientService.Add should be called with new client");
        }

        /// <summary>
        /// A3 Test: Registratie Faalt - Email Al In Gebruik
        /// 
        /// Arrange: Existing email data, client service returns existing client
        /// Act: Execute registration via AuthService
        /// Assert: Verify registration fails and no new client added
        /// </summary>
        [Fact(DisplayName = "A3 Simple - Unhappy Path: Email al in gebruik")]
        [Trait("TestName", "AuthService_EmailAlreadyInUse")]
        public void A3_Simple_AuthService_EmailAlreadyInUse()
        {
            // ===== ARRANGE =====
            // Setup client service to return existing client (email exists)
            var existingClient = new Client(1, "Bestaande Gebruiker", ExistingEmail, "existingpassword");
            _mockClientService.Setup(s => s.Get(ExistingEmail))
                             .Returns(existingClient);

            // Create AuthService with mocked dependencies
            var authService = new Grocery.Core.Services.AuthService(_mockClientService.Object);

            // ===== ACT =====
            var result = authService.Register(ValidName, ExistingEmail, ValidPassword);

            // ===== ASSERT =====
            Assert.False(result, "Registration should fail when email already exists");
            
            // Verify client service was called to check email existence
            _mockClientService.Verify(s => s.Get(ExistingEmail), Times.Once, 
                "ClientService.Get should be called to check email existence");
            
            // Verify client service was NOT called to add new client
            _mockClientService.Verify(s => s.Add(It.IsAny<Client>()), Times.Never, 
                "ClientService.Add should not be called when email already exists");
        }

        #endregion

        #region A3 Test Methods - Password Security Tests

        /// <summary>
        /// A3 Test: Wachtwoord Hashing Verificatie
        /// 
        /// Arrange: Valid user data with plain text password
        /// Act: Execute registration via AuthService
        /// Assert: Verify password is hashed before storage
        /// </summary>
        [Fact(DisplayName = "A3 Simple - Security: Wachtwoord hashing verificatie")]
        [Trait("TestName", "Security_PasswordHashing")]
        public void A3_Simple_Security_PasswordHashing()
        {
            // ===== ARRANGE =====
            var plainTextPassword = ValidPassword;
            
            // Setup client service to return null (email not exists)
            _mockClientService.Setup(s => s.Get(ValidEmail))
                             .Returns((Client)null);
            
            // Setup client service to capture the added client
            Client capturedClient = null;
            _mockClientService.Setup(s => s.Add(It.IsAny<Client>()))
                             .Callback<Client>(client => capturedClient = client)
                             .Returns(new Client(1, ValidName, ValidEmail, "hashedpassword"));

            // Create AuthService with mocked dependencies
            var authService = new Grocery.Core.Services.AuthService(_mockClientService.Object);

            // ===== ACT =====
            var result = authService.Register(ValidName, ValidEmail, plainTextPassword);

            // ===== ASSERT =====
            Assert.True(result, "Registration should be successful");
            Assert.NotNull(capturedClient);
            
            // Verify password is hashed (not plain text)
            Assert.NotEqual(plainTextPassword, capturedClient.Password);
            Assert.True(capturedClient.Password.Length > plainTextPassword.Length);
            
            // Verify password is not empty
            Assert.False(string.IsNullOrEmpty(capturedClient.Password));
        }

        /// <summary>
        /// A3 Test: Password Helper Hashing Functionaliteit
        /// 
        /// Arrange: Test password data
        /// Act: Hash password using PasswordHelper
        /// Assert: Verify hashing produces consistent results
        /// </summary>
        [Fact(DisplayName = "A3 Simple - Security: PasswordHelper hashing functionaliteit")]
        [Trait("TestName", "Security_PasswordHelperHashing")]
        public void A3_Simple_Security_PasswordHelperHashing()
        {
            // ===== ARRANGE =====
            var testPassword = "TestWachtwoord123";
            var anotherPassword = "AnderWachtwoord456";

            // ===== ACT =====
            var hash1 = Grocery.Core.Helpers.PasswordHelper.HashPassword(testPassword);
            var hash2 = Grocery.Core.Helpers.PasswordHelper.HashPassword(testPassword);
            var hash3 = Grocery.Core.Helpers.PasswordHelper.HashPassword(anotherPassword);

            // ===== ASSERT =====
            // Verify hash is not empty
            Assert.False(string.IsNullOrEmpty(hash1));
            Assert.False(string.IsNullOrEmpty(hash2));
            Assert.False(string.IsNullOrEmpty(hash3));
            
            // Verify hash is different from original password
            Assert.NotEqual(testPassword, hash1);
            Assert.NotEqual(anotherPassword, hash3);
            
            // Verify same password produces different hashes (due to salt)
            Assert.NotEqual(hash1, hash2);
            
            // Verify different passwords produce different hashes
            Assert.NotEqual(hash1, hash3);
            
            // Verify hash length is reasonable (PBKDF2 typically produces 64+ character hashes)
            Assert.True(hash1.Length > 50);
        }

        #endregion

        #region A3 Test Methods - Client Service Tests

        /// <summary>
        /// A3 Test: Client Service - Get Client by Email
        /// 
        /// Arrange: Mock client service with test data
        /// Act: Get client by email
        /// Assert: Verify correct client is returned
        /// </summary>
        [Fact(DisplayName = "A3 Simple - ClientService: Get client by email")]
        [Trait("TestName", "ClientService_GetByEmail")]
        public void A3_Simple_ClientService_GetByEmail()
        {
            // ===== ARRANGE =====
            var expectedClient = new Client(1, ValidName, ValidEmail, "hashedpassword");
            
            _mockClientService.Setup(s => s.Get(ValidEmail))
                             .Returns(expectedClient);

            // ===== ACT =====
            var result = _mockClientService.Object.Get(ValidEmail);

            // ===== ASSERT =====
            Assert.NotNull(result);
            Assert.Equal(expectedClient.Id, result.Id);
            Assert.Equal(expectedClient.Name, result.Name);
            Assert.Equal(expectedClient.EmailAddress, result.EmailAddress);
            Assert.Equal(expectedClient.Password, result.Password);
            
            // Verify service was called with correct email
            _mockClientService.Verify(s => s.Get(ValidEmail), Times.Once, 
                "ClientService.Get should be called with correct email");
        }

        /// <summary>
        /// A3 Test: Client Service - Add New Client
        /// 
        /// Arrange: Mock client service
        /// Act: Add new client
        /// Assert: Verify client is added and returned
        /// </summary>
        [Fact(DisplayName = "A3 Simple - ClientService: Add new client")]
        [Trait("TestName", "ClientService_AddClient")]
        public void A3_Simple_ClientService_AddClient()
        {
            // ===== ARRANGE =====
            var newClient = new Client(0, ValidName, ValidEmail, "hashedpassword");
            var expectedClient = new Client(1, ValidName, ValidEmail, "hashedpassword");
            
            _mockClientService.Setup(s => s.Add(It.IsAny<Client>()))
                             .Returns(expectedClient);

            // ===== ACT =====
            var result = _mockClientService.Object.Add(newClient);

            // ===== ASSERT =====
            Assert.NotNull(result);
            Assert.Equal(expectedClient.Id, result.Id);
            Assert.Equal(expectedClient.Name, result.Name);
            Assert.Equal(expectedClient.EmailAddress, result.EmailAddress);
            Assert.Equal(expectedClient.Password, result.Password);
            
            // Verify service was called with correct client
            _mockClientService.Verify(s => s.Add(It.Is<Client>(c => 
                c.Name == newClient.Name && 
                c.EmailAddress == newClient.EmailAddress)), Times.Once, 
                "ClientService.Add should be called with correct client");
        }

        #endregion

        #region A3 Test Methods - Integration Tests

        /// <summary>
        /// A3 Test: Complete Registration Flow Integration
        /// 
        /// Arrange: Complete valid registration data with all services
        /// Act: Execute full registration flow
        /// Assert: Verify complete integration success
        /// </summary>
        [Fact(DisplayName = "A3 Simple - Integration: Complete registratie flow")]
        [Trait("TestName", "Integration_CompleteRegistrationFlow")]
        public void A3_Simple_Integration_CompleteRegistrationFlow()
        {
            // ===== ARRANGE =====
            // Setup client service for complete flow
            _mockClientService.Setup(s => s.Get(ValidEmail))
                             .Returns((Client)null); // Email doesn't exist
            
            Client capturedClient = null;
            _mockClientService.Setup(s => s.Add(It.IsAny<Client>()))
                             .Callback<Client>(client => capturedClient = client)
                             .Returns(new Client(1, ValidName, ValidEmail, "hashedpassword"));

            // Create AuthService with mocked dependencies
            var authService = new Grocery.Core.Services.AuthService(_mockClientService.Object);

            // ===== ACT =====
            var result = authService.Register(ValidName, ValidEmail, ValidPassword);

            // ===== ASSERT =====
            // Verify complete success
            Assert.True(result, "Complete registration flow should be successful");
            
            // Verify email existence check
            _mockClientService.Verify(s => s.Get(ValidEmail), Times.Once, 
                "Email existence should be checked");
            
            // Verify client addition
            Assert.NotNull(capturedClient);
            Assert.Equal(ValidName, capturedClient.Name);
            Assert.Equal(ValidEmail, capturedClient.EmailAddress);
            Assert.NotEqual(ValidPassword, capturedClient.Password); // Should be hashed
            
            _mockClientService.Verify(s => s.Add(It.IsAny<Client>()), Times.Once, 
                "New client should be added");
        }

        #endregion

        #region A3 Test Methods - Edge Cases

        /// <summary>
        /// A3 Test: Null Input Handling
        /// 
        /// Arrange: Null input data
        /// Act: Execute registration with null inputs
        /// Assert: Verify appropriate handling of null inputs
        /// </summary>
        [Theory(DisplayName = "A3 Simple - Edge Cases: Null input handling")]
        [InlineData(null, "test@example.com", "password")]
        [InlineData("Test", null, "password")]
        [InlineData("Test", "test@example.com", null)]
        [InlineData("", "test@example.com", "password")]
        [InlineData("Test", "", "password")]
        [InlineData("Test", "test@example.com", "")]
        [Trait("TestName", "EdgeCases_NullInputHandling")]
        public void A3_Simple_EdgeCases_NullInputHandling(string name, string email, string password)
        {
            // ===== ARRANGE =====
            var authService = new Grocery.Core.Services.AuthService(_mockClientService.Object);

            // ===== ACT =====
            var result = authService.Register(name, email, password);

            // ===== ASSERT =====
            Assert.False(result, "Registration should fail with null or empty inputs");
            
            // Verify no service calls were made
            _mockClientService.Verify(s => s.Get(It.IsAny<string>()), Times.Never, 
                "No service calls should be made with invalid inputs");
            _mockClientService.Verify(s => s.Add(It.IsAny<Client>()), Times.Never, 
                "No client should be added with invalid inputs");
        }

        #endregion
    }
}
