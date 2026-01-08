using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShopAPI.Models;

/// <summary>
/// Represents a fixed-amount discount code that can be applied to orders.
/// Each discount is owned by a specific user (shop) and has usage limits.
/// </summary>
public class Discount
{
    /// <summary>
    /// Primary key, auto-incremented by the database.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DiscountId { get; set; }

    /// <summary>
    /// The discount code (e.g., "SAVE50K", "WELCOME10").
    /// Must be unique per shop (UserId).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the discount.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Fixed amount to deduct from the order (in the same currency as the order).
    /// Must be greater than 0.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int Amount { get; set; }

    /// <summary>
    /// Start date of the discount validity period.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// End date of the discount validity period.
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);

    /// <summary>
    /// Maximum number of times this discount code can be used (across all orders).
    /// Null means unlimited usage.
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// Current number of times this discount has been used.
    /// </summary>
    [Required]
    public int UsedCount { get; set; } = 0;

    /// <summary>
    /// Whether this discount is active and can be used.
    /// Set to false to deactivate without deleting.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Owner of this discount (Shop ID).
    /// Used for multi-tenant data isolation.
    /// Set by the controller based on authenticated user.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    // ====================================================
    // Navigation Properties
    // ====================================================

    /// <summary>
    /// The user (shop owner) who created this discount.
    /// Navigation property - loaded by EF Core when needed.
    /// </summary>
    public ApplicationUser? User { get; set; }

    // ====================================================
    // Helper Properties
    // ====================================================

    /// <summary>
    /// Check if the discount is currently valid (active, not expired, and within usage limit).
    /// </summary>
    [NotMapped]
    public bool IsValid
    {
        get
        {
            if (!IsActive) return false;
            
            var now = DateTime.UtcNow;
            var isWithinDateRange = now >= StartDate && now <= EndDate;
            var isWithinUsageLimit = !UsageLimit.HasValue || UsedCount < UsageLimit.Value;
            return isWithinDateRange && isWithinUsageLimit;
        }
    }

    /// <summary>
    /// Check if the discount has expired.
    /// </summary>
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > EndDate;

    /// <summary>
    /// Check if usage limit has been reached.
    /// </summary>
    [NotMapped]
    public bool IsLimitReached => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;
}
