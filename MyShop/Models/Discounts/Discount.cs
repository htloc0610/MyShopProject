using System;

namespace MyShop.Models.Discounts;

/// <summary>
/// Represents a discount code in the WinUI application.
/// Matches the API model structure.
/// </summary>
public class Discount
{
    public int DiscountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Amount { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string UserId { get; set; } = string.Empty;

    // Helper properties for UI
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

    public bool IsExpired => DateTime.UtcNow > EndDate;
    
    public bool IsLimitReached => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;

    public string FormattedAmount => $"{Amount:N0} ₫";

    public string FormattedStartDate => StartDate.ToLocalTime().ToString("dd/MM/yyyy");
    
    public string FormattedEndDate => EndDate.ToLocalTime().ToString("dd/MM/yyyy");

    public string Status
    {
        get
        {
            if (!IsActive) return "Đã tắt";
            if (IsExpired) return "Hết hạn";
            if (IsLimitReached) return "Đã hết lượt";
            if (IsValid) return "Đang hoạt động";
            return "Đã tắt";
        }
    }

    public string UsageDisplay =>
        UsageLimit.HasValue 
            ? $"{UsedCount}/{UsageLimit}" 
            : $"{UsedCount}/Không giới hạn";
}
