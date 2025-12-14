using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Customers;
using MyShopServer.Infrastructure.Data;
using MyShopServer.Application.Common;

namespace MyShopServer.Application.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(CustomerQueryOptions options, CancellationToken ct = default)
    {
        var page = Math.Max(1, options.Page);
        var pageSize = Math.Clamp(options.PageSize, 1, 100);

        var baseQuery = _db.Customers
            .AsNoTracking()
            .Include(c => c.Orders)
            .OrderByDescending(c => c.CustomerId);

        // nếu không search thì paging như bình thường
        if (string.IsNullOrWhiteSpace(options.Search))
        {
            var total = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    OrderCount = c.Orders.Count
                })
                .ToListAsync(ct);

            return new PagedResult<CustomerDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = items
            };
        }

        // có search: lọc in-memory để hỗ trợ bỏ dấu
        var term = TextSearch.Normalize(options.Search);

        // NOTE: load hết rồi lọc -> chỉ nên dùng khi data nhỏ/vừa
        var all = await baseQuery
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                OrderCount = c.Orders.Count
            })
            .ToListAsync(ct);

        var filtered = all.Where(c =>
        {
            var key = TextSearch.Normalize($"{c.Name} {c.Phone} {c.Email}");
            return key.Contains(term);
        }).ToList();

        var total2 = filtered.Count;
        var items2 = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<CustomerDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total2,
            Items = items2
        };
    }


    public async Task<CustomerDto?> GetByIdAsync(int customerId, CancellationToken ct = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == customerId)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                OrderCount = c.Orders.Count
            })
            .SingleOrDefaultAsync(ct);
    }

    public async Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new Exception("Name is required.");
        if (string.IsNullOrWhiteSpace(dto.Phone)) throw new Exception("Phone is required.");

        // optional: unique phone
        var phoneExists = await _db.Customers.AnyAsync(c => c.Phone == dto.Phone, ct);
        if (phoneExists) throw new Exception($"Phone '{dto.Phone}' already exists.");

        var entity = new Domain.Entities.Customer
        {
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
        };

        _db.Customers.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CustomerDto
        {
            CustomerId = entity.CustomerId,
            Name = entity.Name,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            OrderCount = 0
        };
    }

    public async Task<CustomerDto> UpdateAsync(int customerId, CustomerDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Customers.SingleOrDefaultAsync(c => c.CustomerId == customerId, ct);
        if (entity == null) throw new Exception("Customer not found.");

        // optional: unique phone (nếu đổi)
        if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != entity.Phone)
        {
            var phoneExists = await _db.Customers.AnyAsync(c => c.Phone == dto.Phone && c.CustomerId != customerId, ct);
            if (phoneExists) throw new Exception($"Phone '{dto.Phone}' already exists.");
        }

        entity.Name = string.IsNullOrWhiteSpace(dto.Name) ? entity.Name : dto.Name.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? entity.Phone : dto.Phone.Trim();
        entity.Email = dto.Email == null ? entity.Email : (string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim());
        entity.Address = dto.Address == null ? entity.Address : (string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim());

        await _db.SaveChangesAsync(ct);

        // count orders
        var orderCount = await _db.Orders.CountAsync(o => o.CustomerId == entity.CustomerId, ct);

        return new CustomerDto
        {
            CustomerId = entity.CustomerId,
            Name = entity.Name,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            OrderCount = orderCount
        };
    }

    public async Task<bool> DeleteAsync(int customerId, CancellationToken ct = default)
    {
        // nếu customer có order → tùy rule: không cho xóa
        var hasOrders = await _db.Orders.AnyAsync(o => o.CustomerId == customerId, ct);
        if (hasOrders)
            throw new Exception("Cannot delete customer because it has related orders.");

        var entity = await _db.Customers.SingleOrDefaultAsync(c => c.CustomerId == customerId, ct);
        if (entity == null) return false;

        _db.Customers.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
