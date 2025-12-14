using MyShopServer.DTOs.Common;

namespace MyShopServer.DTOs.Customers;

public class CustomerResultDto : MutationResult<CustomerDto> { }
public class CustomerListResultDto : MutationResult<PagedResult<CustomerDto>> { }
