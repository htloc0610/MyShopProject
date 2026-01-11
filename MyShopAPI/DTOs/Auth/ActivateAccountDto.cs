namespace MyShopAPI.DTOs.Auth
{
    /// <summary>
    /// DTO for activating an account with a license key.
    /// </summary>
    public class ActivateAccountDto
    {
        /// <summary>
        /// The 6-character activation code to validate.
        /// </summary>
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for account activation.
    /// </summary>
    public class ActivateAccountResponseDto
    {
        /// <summary>
        /// Whether the activation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
