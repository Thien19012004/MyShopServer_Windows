using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Common;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Common;
using MyShopServer.Domain.Entities;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Users;
using MyShopServer.Infrastructure.Data;

namespace MyShopServer.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(UserQueryOptions options, CancellationToken ct = default)
    {
        var page = options.Page <= 0 ? 1 : options.Page;
        var pageSize = options.PageSize <= 0 ? 10 : Math.Clamp(options.PageSize, 1, 100);

        var query = _db.Users
        .AsNoTracking()
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .AsQueryable();

        if (options.IsActive.HasValue)
            query = query.Where(u => u.IsActive == options.IsActive.Value);

        if (options.Role.HasValue)
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == options.Role.Value));

        query = query.OrderByDescending(u => u.UserId);

        if (string.IsNullOrWhiteSpace(options.Search))
        {
            var total = await query.CountAsync(ct);
            var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName ?? string.Empty,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Roles = u.UserRoles.Select(x => x.Role.RoleName).ToList()
            })
            .ToListAsync(ct);

            return new PagedResult<UserDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = items
            };
        }

        // search accent-insensitive (like CustomerService)
        var term = TextSearch.Normalize(options.Search);
        var raw = await query
        .Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Roles = u.UserRoles.Select(x => x.Role.RoleName).ToList()
        })
        .ToListAsync(ct);

        var filtered = raw
        .Where(u => TextSearch.Normalize($"{u.Username} {u.FullName}").Contains(term))
        .ToList();

        var total2 = filtered.Count;
        var items2 = filtered
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

        return new PagedResult<UserDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total2,
            Items = items2
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken ct = default)
    {
        return await _db.Users
        .AsNoTracking()
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .Where(u => u.UserId == userId)
        .Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Roles = u.UserRoles.Select(x => x.Role.RoleName).ToList()
        })
        .SingleOrDefaultAsync(ct);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;

        return await _db.Users
        .AsNoTracking()
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .Where(u => u.Username == username)
        .Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Roles = u.UserRoles.Select(x => x.Role.RoleName).ToList()
        })
        .SingleOrDefaultAsync(ct);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Username)) throw new Exception("Username is required.");
        if (string.IsNullOrWhiteSpace(dto.Password)) throw new Exception("Password is required.");
        if (string.IsNullOrWhiteSpace(dto.FullName)) throw new Exception("FullName is required.");
        if (dto.Roles == null || dto.Roles.Count == 0) throw new Exception("At least one role is required.");

        var username = dto.Username.Trim();

        var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Username == username, ct);
        if (exists) throw new Exception($"Username '{username}' already exists.");

        // roles must exist
        var roleNames = dto.Roles.Distinct().ToList();
        var roleEntities = await _db.Roles
        .Where(r => roleNames.Contains(r.RoleName))
        .ToListAsync(ct);

        if (roleEntities.Count != roleNames.Count)
            throw new Exception("Some roles do not exist.");

        var entity = new User
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FullName = dto.FullName.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UserRoles = roleEntities.Select(r => new UserRole { RoleId = r.RoleId }).ToList()
        };

        _db.Users.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetUserByIdAsync(entity.UserId, ct)
        ?? throw new Exception("Failed to load created user.");
    }

    public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Users
        .Include(u => u.UserRoles)
        .SingleOrDefaultAsync(u => u.UserId == userId, ct);

        if (entity == null) return null;

        if (dto.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName)) throw new Exception("FullName cannot be empty.");
            entity.FullName = dto.FullName.Trim();
        }

        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;

        if (dto.Roles != null)
        {
            if (dto.Roles.Count == 0) throw new Exception("Roles cannot be empty.");

            var roleNames = dto.Roles.Distinct().ToList();
            var roleEntities = await _db.Roles
            .Where(r => roleNames.Contains(r.RoleName))
            .ToListAsync(ct);

            if (roleEntities.Count != roleNames.Count)
                throw new Exception("Some roles do not exist.");

            _db.UserRoles.RemoveRange(entity.UserRoles);
            entity.UserRoles = roleEntities.Select(r => new UserRole { UserId = userId, RoleId = r.RoleId }).ToList();
        }

        await _db.SaveChangesAsync(ct);
        return await GetUserByIdAsync(userId, ct);
    }

    public async Task<bool> DeactivateUserAsync(int userId, CancellationToken ct = default)
    {
        return await SetUserActiveAsync(userId, isActive: false, ct);
    }

    public async Task<bool> SetUserActiveAsync(int userId, bool isActive, CancellationToken ct = default)
    {
        var entity = await _db.Users.SingleOrDefaultAsync(u => u.UserId == userId, ct);
        if (entity == null) return false;

        entity.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword)) throw new Exception("New password is required.");

        var entity = await _db.Users.SingleOrDefaultAsync(u => u.UserId == userId, ct);
        if (entity == null) return false;

        entity.PasswordHash = PasswordHasher.Hash(newPassword);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
