using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Customers;

namespace MyShopServer.Application.Services.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerQueryOptions options, CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(int customerId, CancellationToken ct = default);

    Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken ct = default);
    Task<CustomerDto> UpdateAsync(int customerId, CustomerDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int customerId, CancellationToken ct = default);
}
