using System.ComponentModel.DataAnnotations;

namespace MyShopAPI.DTOs
{
    /// <summary>
    /// Response DTO for Customer data.
    /// </summary>
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
        public long TotalSpent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for creating a new customer.
    /// </summary>
    public class CustomerCreateDto
    {
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        public string? Address { get; set; }

        public DateTime? Birthday { get; set; }
    }

    /// <summary>
    /// Request DTO for updating an existing customer.
    /// </summary>
    public class CustomerUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        public string? Address { get; set; }

        public DateTime? Birthday { get; set; }

        public long TotalSpent { get; set; }
    }
}
