using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
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
            Debug.WriteLine("=== AuthService Initialized ===");
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
                Debug.WriteLine($"=== Register Request: {request.Email}, Shop: {request.ShopName} ===");
                Debug.WriteLine($"=== Register Response: {response.StatusCode} ===");

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
                    if (tokenResponse != null)
                    {
                        Debug.WriteLine("=== Register Success: Token received ===");
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
                Debug.WriteLine($"=== Register Error Content: {errorContent} ===");
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = ExtractErrorMessage(errorContent) ?? "Registration failed"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== Register Exception: {ex.Message} ===");
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

                        // Persist new tokens to vault since we used vault to refresh
                        // This maintains the "Remember Me" chain.
                        _credentialService.SaveAccessToken(tokenResponse.AccessToken);
                        _credentialService.SaveRefreshToken(tokenResponse.RefreshToken);

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
        /// <summary>
        /// Extract error message from API response.
        /// </summary>
        private static string? ExtractErrorMessage(string content)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Check for 'errors' array (ASP.NET Identity format)
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
                {
                    var errorMessages = new System.Collections.Generic.List<string>();
                    foreach (var error in errors.EnumerateArray())
                    {
                        if (error.TryGetProperty("description", out var description))
                        {
                            errorMessages.Add(description.GetString() ?? string.Empty);
                        }
                    }

                    if (errorMessages.Count > 0)
                    {
                        return string.Join(Environment.NewLine, errorMessages);
                    }
                }

                // Fallback to 'message' property
                if (root.TryGetProperty("message", out var message) ||
                    root.TryGetProperty("Message", out message))
                {
                    return message.GetString();
                }
            }
            catch
            {
                // Ignore parsing errors and return raw content if simple string
                if (!string.IsNullOrWhiteSpace(content) && !content.TrimStart().StartsWith("{"))
                {
                    return content;
                }
            }
            return null;
        }
    }
}
