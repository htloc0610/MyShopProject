using System;

namespace MyShop.Models.Customers
{
    /// <summary>
    /// Client-side model for Customer data.
    /// </summary>
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
        public long TotalSpent { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// First character of name for avatar display.
        /// </summary>
        public string NameInitial => !string.IsNullOrEmpty(Name) ? Name[0].ToString().ToUpper() : "?";

        /// <summary>
        /// Formatted phone number for display.
        /// </summary>
        public string FormattedPhone => PhoneNumber;

        /// <summary>
        /// Formatted total spent with currency.
        /// </summary>
        public string FormattedTotalSpent => $"{TotalSpent:N0} ₫";

        /// <summary>
        /// Formatted birthday for display.
        /// </summary>
        public string FormattedBirthday => Birthday?.ToString("dd/MM/yyyy") ?? "Không có";

        /// <summary>
        /// Formatted address for display.
        /// </summary>
        public string FormattedAddress => Address ?? "Không có";

        /// <summary>
        /// Get loyalty status based on total spent.
        /// </summary>
        public string LoyaltyStatus
        {
            get
            {
                return TotalSpent switch
                {
                    >= 10_000_000 => "💎 VIP",
                    >= 5_000_000 => "🥇 Vàng",
                    >= 2_000_000 => "🥈 Bạc",
                    >= 500_000 => "🥉 Đồng",
                    _ => "🌟 Mới"
                };
            }
        }

        /// <summary>
        /// Loyalty tier color.
        /// </summary>
        public string LoyaltyColor
        {
            get
            {
                return TotalSpent switch
                {
                    >= 10_000_000 => "#9B59B6",  // Purple - VIP
                    >= 5_000_000 => "#F1C40F",   // Gold
                    >= 2_000_000 => "#95A5A6",   // Silver
                    >= 500_000 => "#CD7F32",     // Bronze
                    _ => "#3498DB"                // Blue - New
                };
            }
        }
    }

    /// <summary>
    /// DTO for creating a new customer.
    /// </summary>
    public class CustomerCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing customer.
    /// </summary>
    public class CustomerUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
        public long TotalSpent { get; set; }
    }
}
