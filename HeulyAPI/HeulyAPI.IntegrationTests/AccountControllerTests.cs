using HeulyAPI.Dtos.Account;
using HeulyAPI.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace HeulyAPI.IntegrationTests
{
    public class AccountControllerTests : IClassFixture<MockDb<Program>>
    {
        private readonly HttpClient _client;
        private readonly MockDb<Program> _factory;

        public AccountControllerTests(MockDb<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ReturnsSuccessStatusCode()
        {
            var registerDto = new
            {
                Email = "olaMundot123@gmail.com",
                Name = "hellot",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };

            var response = await _client.PostAsJsonAsync("/api/Account/register", registerDto);

            response.EnsureSuccessStatusCode();

            var responseDto = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            Assert.True(responseDto.IsSuccess);
            Assert.NotNull(responseDto.Token);
        }

        [Fact]
        public async Task Login_ReturnsToken()
        {
            // Ensure the user is registered first
            var registerDto = new
            {
                Email = "olaMundo123@gmail.com",
                Name = "hello",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };
            await _client.PostAsJsonAsync("/api/Account/register", registerDto);

            var loginDto = new
            {
                Email = "olaMundo123@gmail.com",
                Password = "Test@123"
            };

            var response = await _client.PostAsJsonAsync("/api/Account/login", loginDto);

            response.EnsureSuccessStatusCode();
            var responseDto = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            Assert.True(responseDto.IsSuccess);
            Assert.NotNull(responseDto.Token); // Verifica se o token não é nulo
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnOk_WhenEmailIsValid()
        {
            // Arrange
            // Register a user first
            var registerDto = new RegisterDto
            {
                Email = "olaMundo123@gmail.com",
                Name = "hello",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };
            var registerContent = new StringContent(
                JsonSerializer.Serialize(registerDto),
                Encoding.UTF8,
                "application/json");

            var registerResponse = await _client.PostAsync("/api/Account/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            // Act
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "olaMundo123@gmail.com"
            };
            var forgotPasswordContent = new StringContent(
                JsonSerializer.Serialize(forgotPasswordDto),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/Account/forgot-password", forgotPasswordContent);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Assert.True(false, $"HTTP request failed. Status code: {response.StatusCode}, Content: {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<AuthResponseDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(responseObject);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal("Um link para redefinir a senha foi enviado para seu e-mail.", responseObject.Message);
        }


        [Fact]
        public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "non-existing-user@example.com"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(forgotPasswordDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/Account/forgot-password", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<AuthResponseDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(responseObject);
            Assert.False(responseObject.IsSuccess);
            Assert.Equal("Se um usuário com esse e-mail existir, um e-mail de redefinição será enviado.", responseObject.Message);
        }

        [Fact]
        public async Task RegistrationExistingUserTest()
        {
            var registerDto = new
            {
                Email = "existinguser@example.com",
                Name = "existinguser",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };

            // Primeiro registro
            var response = await _client.PostAsJsonAsync("/api/Account/register", registerDto);
            response.EnsureSuccessStatusCode(); 

            var duplicateRegisterResponse = await _client.PostAsJsonAsync("/api/Account/register", registerDto);

            Assert.Equal(HttpStatusCode.BadRequest, duplicateRegisterResponse.StatusCode);

            var responseString = await duplicateRegisterResponse.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<List<IdentityError>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(responseObject);
            Assert.Contains(responseObject, e => e.Code == "DuplicateUserName");
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_ForWeakPassword()
        {
            var registerDto = new
            {
                Email = "userWithWeakPassword@gmail.com",
                Name = "weakpassword",
                Password = "123", 
                ConfirmPassword = "123"
            };

            var response = await _client.PostAsJsonAsync("/api/Account/register", registerDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<List<IdentityError>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(responseObject);
            Assert.Contains(responseObject, e => e.Code == "PasswordTooShort");
        }



    }
}