using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MyShop.Models.Auth;

namespace MyShop.Services.Auth
{
    /// <summary>
    /// Implementation of IAuthService for API authentication.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ISessionService _sessionService;
        private readonly ICredentialService _credentialService;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthService(
            HttpClient httpClient,
            ISessionService sessionService,
            ICredentialService credentialService)
        {
            _httpClient = httpClient;
            _sessionService = sessionService;
            _credentialService = credentialService;

            // Configure base address for auth API
            _httpClient.BaseAddress = new Uri("http://localhost:5002/");
        }

        /// <inheritdoc />
        public async Task<AuthResult> RegisterAsync(string email, string password, string shopName, string? role = null)
        {
            try
            {
                var request = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    ShopName = shopName,
                    Role = role ?? "Owner"
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
                    if (tokenResponse != null)
                    {
                        _sessionService.SetSession(
                            tokenResponse.User.Id,
                            tokenResponse.User.Email,
                            tokenResponse.User.ShopName,
                            tokenResponse.User.Role,
                            tokenResponse.AccessToken,
                            tokenResponse.RefreshToken);

                        return new AuthResult
                        {
                            Success = true,
                            User = tokenResponse.User
                        };
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = ExtractErrorMessage(errorContent) ?? "Registration failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Connection error: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
                    if (tokenResponse != null)
                    {
                        _sessionService.SetSession(
                            tokenResponse.User.Id,
                            tokenResponse.User.Email,
                            tokenResponse.User.ShopName,
                            tokenResponse.User.Role,
                            tokenResponse.AccessToken,
                            tokenResponse.RefreshToken);

                        return new AuthResult
                        {
                            Success = true,
                            User = tokenResponse.User
                        };
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = ExtractErrorMessage(errorContent) ?? "Invalid email or password"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Connection error: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<AuthResult> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = _credentialService.GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "No refresh token available"
                    };
                }

                var request = new RefreshTokenRequest { RefreshToken = refreshToken };
                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", request);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
                    if (tokenResponse != null)
                    {
                        _sessionService.SetSession(
                            tokenResponse.User.Id,
                            tokenResponse.User.Email,
                            tokenResponse.User.ShopName,
                            tokenResponse.User.Role,
                            tokenResponse.AccessToken,
                            tokenResponse.RefreshToken);

                        return new AuthResult
                        {
                            Success = true,
                            User = tokenResponse.User
                        };
                    }
                }

                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Token refresh failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = $"Connection error: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task LogoutAsync()
        {
            try
            {
                var accessToken = _sessionService.AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    
                    await _httpClient.PostAsync("api/auth/logout", null);
                }
            }
            catch
            {
                // Ignore errors during logout
            }
            finally
            {
                _sessionService.ClearSession();
            }
        }

        /// <inheritdoc />
        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            try
            {
                var accessToken = _sessionService.AccessToken;
                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync("api/auth/me");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserInfo>(JsonOptions);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract error message from API response.
        /// </summary>
        private static string? ExtractErrorMessage(string content)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var message) ||
                    doc.RootElement.TryGetProperty("Message", out message))
                {
                    return message.GetString();
                }
            }
            catch
            {
                // Ignore parsing errors
            }
            return null;
        }
    }
}
