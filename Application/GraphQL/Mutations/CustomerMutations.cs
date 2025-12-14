using HotChocolate.Types;
using MyShopServer.Application.GraphQL.Inputs.Customers;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Customers;

namespace MyShopServer.Application.GraphQL.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class CustomerMutations
{
    public async Task<CustomerResultDto> CreateCustomer(
        CreateCustomerInput input,
        [Service] ICustomerService customerService,
        CancellationToken ct)
    {
        try
        {
            var dto = new CustomerDto
            {
                Name = input.Name,
                Phone = input.Phone,
                Email = input.Email,
                Address = input.Address
            };

            var created = await customerService.CreateAsync(dto, ct);

            return new CustomerResultDto
            {
                StatusCode = 201,
                Success = true,
                Message = "Customer created successfully",
                Data = created
            };
        }
        catch (Exception ex)
        {
            return new CustomerResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    public async Task<CustomerResultDto> UpdateCustomer(
        int customerId,
        UpdateCustomerInput input,
        [Service] ICustomerService customerService,
        CancellationToken ct)
    {
        try
        {
            var existing = await customerService.GetByIdAsync(customerId, ct);
            if (existing == null)
            {
                return new CustomerResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Customer not found",
                    Data = null
                };
            }

            var dto = new CustomerDto
            {
                CustomerId = customerId,
                Name = input.Name ?? existing.Name,
                Phone = input.Phone ?? existing.Phone,
                Email = input.Email ?? existing.Email,
                Address = input.Address ?? existing.Address,
                OrderCount = existing.OrderCount
            };

            var updated = await customerService.UpdateAsync(customerId, dto, ct);

            return new CustomerResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Customer updated successfully",
                Data = updated
            };
        }
        catch (Exception ex)
        {
            return new CustomerResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    public async Task<CustomerResultDto> DeleteCustomer(
        int customerId,
        [Service] ICustomerService customerService,
        CancellationToken ct)
    {
        try
        {
            var ok = await customerService.DeleteAsync(customerId, ct);

            if (!ok)
            {
                return new CustomerResultDto
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "Customer not found",
                    Data = null
                };
            }

            return new CustomerResultDto
            {
                StatusCode = 200,
                Success = true,
                Message = "Customer deleted successfully",
                Data = null
            };
        }
        catch (Exception ex)
        {
            return new CustomerResultDto
            {
                StatusCode = 400,
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
