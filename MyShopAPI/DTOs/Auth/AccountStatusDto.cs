namespace MyShopAPI.DTOs.Auth
{
    /// <summary>
    /// Enum representing the account status based on trial period and licensing.
    /// </summary>
    public enum AccountStatus
    {
        /// <summary>
        /// Account is licensed and fully active.
        /// </summary>
        Active,

        /// <summary>
        /// Account is in trial period (within 15 days of creation).
        /// </summary>
        Trial,

        /// <summary>
        /// Trial period has expired and account needs activation.
        /// </summary>
        Expired
    }

    /// <summary>
    /// DTO containing account status information for trial/licensing.
    /// </summary>
    public class AccountStatusDto
    {
        /// <summary>
        /// Current status of the account.
        /// </summary>
        public AccountStatus Status { get; set; }

        /// <summary>
        /// Number of days remaining in trial period (0 if expired or licensed).
        /// </summary>
        public int DaysRemaining { get; set; }

        /// <summary>
        /// Whether the account has been activated with a license key.
        /// </summary>
        public bool IsLicensed { get; set; }
    }
}
