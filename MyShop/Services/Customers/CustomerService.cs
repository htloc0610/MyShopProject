using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyShop.Models.Customers;
using MyShop.Models.Shared;

namespace MyShop.Services.Customers
{
    /// <summary>
    /// Service for managing customer data from API.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private const string CustomersEndpoint = "/api/customers";

        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<PagedResult<Customer>> GetCustomersAsync(
            int page = 1,
            int pageSize = 20,
            string? keyword = null,
            string? sortBy = null,
            bool isDescending = false)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}",
                    $"isDescending={isDescending.ToString().ToLower()}"
                };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");
                }

                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
                }

                var url = $"{CustomersEndpoint}?{string.Join("&", queryParams)}";
                var result = await _httpClient.GetFromJsonAsync<PagedResult<Customer>>(url);

                return result ?? new PagedResult<Customer>
                {
                    Items = new List<Customer>(),
                    TotalCount = 0,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting customers: {ex.Message}");
                return new PagedResult<Customer>
                {
                    Items = new List<Customer>(),
                    TotalCount = 0,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
        }

        /// <inheritdoc />
        public async Task<Customer?> GetCustomerByIdAsync(Guid id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Customer>($"{CustomersEndpoint}/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting customer {id}: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<Customer?> CreateCustomerAsync(CustomerCreateDto customer)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(CustomersEndpoint, customer);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Customer>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error creating customer: {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating customer: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<Customer?> UpdateCustomerAsync(Guid id, CustomerUpdateDto customer)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{CustomersEndpoint}/{id}", customer);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Customer>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error updating customer: {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating customer {id}: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{CustomersEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting customer {id}: {ex.Message}");
                return false;
            }
        }
    }
}
