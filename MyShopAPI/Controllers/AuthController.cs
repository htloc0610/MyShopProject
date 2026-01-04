using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyShopAPI.DTOs.Auth;
using MyShopAPI.Models;
using MyShopAPI.Services;

namespace MyShopAPI.Controllers
{
    /// <summary>
    /// Controller for authentication operations (register, login, refresh, logout).
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user (Owner or Staff).
        /// </summary>
        /// <param name="registerDto">Registration details.</param>
        /// <returns>JWT tokens and user info on success.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "User with this email already exists." });
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                ShopName = registerDto.ShopName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Registration failed.", Errors = result.Errors });
            }

            // Assign role (default to Owner if not specified)
            var role = registerDto.Role ?? "Owner";
            if (role != "Owner" && role != "Staff")
            {
                role = "Owner"; // Fallback to Owner if invalid role
            }

            await _userManager.AddToRoleAsync(user, role);

            _logger.LogInformation("User {Email} registered successfully with role {Role}", user.Email, role);

            // Generate tokens
            var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user, role);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    ShopName = user.ShopName,
                    Role = role
                }
            });
        }

        /// <summary>
        /// Login with email and password.
        /// </summary>
        /// <param name="loginDto">Login credentials.</param>
        /// <returns>JWT tokens and user info on success.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // Get user role
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Owner";

            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            // Generate tokens
            var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user, role);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    ShopName = user.ShopName,
                    Role = role
                }
            });
        }

        /// <summary>
        /// Refresh the access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshTokenDto">The refresh token.</param>
        /// <returns>New JWT tokens on success.</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var user = await _tokenService.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid or expired refresh token." });
            }

            // Revoke the old refresh token
            await _tokenService.RevokeTokenAsync(refreshTokenDto.RefreshToken);

            // Get user role
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Owner";

            _logger.LogInformation("Token refreshed for user {Email}", user.Email);

            // Generate new tokens
            var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user, role);
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

            return Ok(new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    ShopName = user.ShopName,
                    Role = role
                }
            });
        }

        /// <summary>
        /// Logout and revoke all refresh tokens for the current user.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _tokenService.RevokeAllUserTokensAsync(userId);

            _logger.LogInformation("User {UserId} logged out successfully", userId);

            return Ok(new { Message = "Logged out successfully." });
        }

        /// <summary>
        /// Get current user information.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Owner";

            return Ok(new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                ShopName = user.ShopName,
                Role = role
            });
        }
    }
}
