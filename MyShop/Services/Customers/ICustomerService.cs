using System;
using System.Threading.Tasks;
using MyShop.Models.Customers;
using MyShop.Models.Shared;

namespace MyShop.Services.Customers
{
    /// <summary>
    /// Interface for Customer service operations.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Get customers with paging and search.
        /// </summary>
        Task<PagedResult<Customer>> GetCustomersAsync(
            int page = 1, 
            int pageSize = 20, 
            string? keyword = null,
            string? sortBy = null,
            bool isDescending = false);

        /// <summary>
        /// Get a single customer by ID.
        /// </summary>
        Task<Customer?> GetCustomerByIdAsync(Guid id);

        /// <summary>
        /// Create a new customer.
        /// </summary>
        Task<Customer?> CreateCustomerAsync(CustomerCreateDto customer);

        /// <summary>
        /// Update an existing customer.
        /// </summary>
        Task<Customer?> UpdateCustomerAsync(Guid id, CustomerUpdateDto customer);

        /// <summary>
        /// Delete a customer.
        /// </summary>
        Task<bool> DeleteCustomerAsync(Guid id);
    }
}
