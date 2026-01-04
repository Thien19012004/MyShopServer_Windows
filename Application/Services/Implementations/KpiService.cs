using Microsoft.EntityFrameworkCore;
using MyShopServer.Application.Services.Interfaces;
using MyShopServer.Domain.Entities;
using MyShopServer.Domain.Enums;
using MyShopServer.DTOs.Common;
using MyShopServer.DTOs.Kpi;
using MyShopServer.Infrastructure.Data;

namespace MyShopServer.Application.Services.Implementations;

public class KpiService : IKpiService
{
    private readonly AppDbContext _db;
    private const int DEFAULT_COMMISSION_PERCENT = 10; // base commission

    public KpiService(AppDbContext db)
    {
        _db = db;
    }

    private static void ValidateYearMonth(int year, int month)
    {
        if (year < 2000 || year > 2100) throw new Exception("Invalid year.");
        if (month < 1 || month > 12) throw new Exception("Invalid month (1..12).");
    }

    private static void ValidateTier(KpiTierDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new Exception("Tier name is required.");
        if (dto.DisplayOrder <= 0) throw new Exception("DisplayOrder must be >0.");

        // NOTE: In this simplified KPI system, MinRevenue is interpreted as MinAchievedPercent (0..1000)
        if (dto.MinRevenue < 0 || dto.MinRevenue > 1000) throw new Exception("MinRevenue (MinAchievedPercent) must be between0 and1000.");
        if (dto.BonusPercent < 0 || dto.BonusPercent > 100) throw new Exception("BonusPercent must be0..100.");
    }

    // =========================================================
    // KPI TIERS
    // =========================================================
    public async Task<PagedResult<KpiTierDto>> GetKpiTiersAsync(int page = 1, int pageSize = 100, CancellationToken ct = default)
    {
        var query = _db.KpiTiers
         .AsNoTracking()
            .OrderBy(x => x.DisplayOrder);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
                 .Select(x => new KpiTierDto
                 {
                     KpiTierId = x.KpiTierId,
                     Name = x.Name,
                     MinRevenue = x.MinRevenue,
                     BonusPercent = x.BonusPercent,
                     Description = x.Description,
                     DisplayOrder = x.DisplayOrder
                 })
               .ToListAsync(ct);

        return new PagedResult<KpiTierDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            Items = items
        };
    }

    public async Task<KpiTierDto?> GetKpiTierByIdAsync(int kpiTierId, CancellationToken ct = default)
    {
        return await _db.KpiTiers
                  .AsNoTracking()
          .Where(x => x.KpiTierId == kpiTierId)
               .Select(x => new KpiTierDto
               {
                   KpiTierId = x.KpiTierId,
                   Name = x.Name,
                   MinRevenue = x.MinRevenue,
                   BonusPercent = x.BonusPercent,
                   Description = x.Description,
                   DisplayOrder = x.DisplayOrder
               })
          .SingleOrDefaultAsync(ct);
    }

    public async Task<KpiTierDto> CreateKpiTierAsync(KpiTierDto dto, CancellationToken ct = default)
    {
        ValidateTier(dto);

        var entity = new KpiTier
        {
            Name = dto.Name.Trim(),
            MinRevenue = dto.MinRevenue,
            BonusPercent = dto.BonusPercent,
            Description = dto.Description?.Trim(),
            DisplayOrder = dto.DisplayOrder
        };

        _db.KpiTiers.Add(entity);
        await _db.SaveChangesAsync(ct);

        dto.KpiTierId = entity.KpiTierId;
        return dto;
    }

    public async Task<KpiTierDto?> UpdateKpiTierAsync(int kpiTierId, KpiTierDto dto, CancellationToken ct = default)
    {
        ValidateTier(dto);

        var entity = await _db.KpiTiers.FindAsync(new object[] { kpiTierId }, ct);
        if (entity == null) return null;

        entity.Name = dto.Name.Trim();
        entity.MinRevenue = dto.MinRevenue;
        entity.BonusPercent = dto.BonusPercent;
        entity.Description = dto.Description?.Trim();
        entity.DisplayOrder = dto.DisplayOrder;

        await _db.SaveChangesAsync(ct);

        return dto;
    }

    public async Task<bool> DeleteKpiTierAsync(int kpiTierId, CancellationToken ct = default)
    {
        var entity = await _db.KpiTiers.FindAsync(new object[] { kpiTierId }, ct);
        if (entity == null) return false;

        _db.KpiTiers.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // =========================================================
    // SALE KPI TARGETS
    // =========================================================
    public async Task<SaleKpiTargetDto?> SetMonthlyTargetAsync(SetMonthlyTargetDto dto, CancellationToken ct = default)
    {
        ValidateYearMonth(dto.Year, dto.Month);
        if (dto.TargetRevenue < 0) throw new Exception("TargetRevenue must be >=0.");

        // Validate sale exists and has Sale role
        var sale = await _db.Users
                  .Include(u => u.UserRoles)
       .ThenInclude(ur => ur.Role)
                  .SingleOrDefaultAsync(u => u.UserId == dto.SaleId, ct);

        if (sale == null)
            throw new Exception($"User {dto.SaleId} not found.");

        if (!sale.UserRoles.Any(ur => ur.Role.RoleName == RoleName.Sale))
            throw new Exception($"User {dto.SaleId} is not a Sale.");

        // Check if target already exists
        var existing = await _db.SaleKpiTargets
         .SingleOrDefaultAsync(x => x.SaleId == dto.SaleId && x.Year == dto.Year && x.Month == dto.Month, ct);

        if (existing != null)
        {
            existing.TargetRevenue = dto.TargetRevenue;
        }
        else
        {
            existing = new SaleKpiTarget
            {
                SaleId = dto.SaleId,
                Year = dto.Year,
                Month = dto.Month,
                TargetRevenue = dto.TargetRevenue,
                ActualRevenue = 0,
                BonusAmount = 0
            };
            _db.SaleKpiTargets.Add(existing);
        }

        await _db.SaveChangesAsync(ct);

        return await GetSaleKpiTargetAsync(dto.SaleId, dto.Year, dto.Month, ct);
    }

    public async Task<SaleKpiTargetDto?> GetSaleKpiTargetAsync(int saleId, int year, int month, CancellationToken ct = default)
    {
        ValidateYearMonth(year, month);

        return await _db.SaleKpiTargets
        .AsNoTracking()
     .Include(x => x.Sale)
            .Include(x => x.KpiTier)
  .Where(x => x.SaleId == saleId && x.Year == year && x.Month == month)
            .Select(x => new SaleKpiTargetDto
            {
                SaleKpiTargetId = x.SaleKpiTargetId,
                SaleId = x.SaleId,
                SaleName = x.Sale.FullName,
                Year = x.Year,
                Month = x.Month,
                TargetRevenue = x.TargetRevenue,
                ActualRevenue = x.ActualRevenue,
                KpiTierId = x.KpiTierId,
                KpiTierName = x.KpiTier != null ? x.KpiTier.Name : null,
                BonusAmount = x.BonusAmount,
                CalculatedAt = x.CalculatedAt,
                CreatedAt = x.CreatedAt
            })
   .SingleOrDefaultAsync(ct);
    }

    public async Task<PagedResult<SaleKpiTargetDto>> GetSaleKpiTargetsAsync(
        int? saleId,
        int? year,
        int? month,
        int page = 1,
     int pageSize = 10,
CancellationToken ct = default)
    {
        if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            throw new Exception("Invalid year.");
        if (month.HasValue && (month.Value < 1 || month.Value > 12))
            throw new Exception("Invalid month (1..12).");

        var query = _db.SaleKpiTargets
       .AsNoTracking()
     .Include(x => x.Sale)
            .Include(x => x.KpiTier)
   .AsQueryable();

        if (saleId.HasValue)
            query = query.Where(x => x.SaleId == saleId.Value);

        if (year.HasValue)
            query = query.Where(x => x.Year == year.Value);

        if (month.HasValue)
            query = query.Where(x => x.Month == month.Value);

        query = query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

        var total = await query.CountAsync(ct);

        var items = await query
         .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .Select(x => new SaleKpiTargetDto
               {
                   SaleKpiTargetId = x.SaleKpiTargetId,
                   SaleId = x.SaleId,
                   SaleName = x.Sale.FullName,
                   Year = x.Year,
                   Month = x.Month,
                   TargetRevenue = x.TargetRevenue,
                   ActualRevenue = x.ActualRevenue,
                   KpiTierId = x.KpiTierId,
                   KpiTierName = x.KpiTier != null ? x.KpiTier.Name : null,
                   BonusAmount = x.BonusAmount,
                   CalculatedAt = x.CalculatedAt,
                   CreatedAt = x.CreatedAt
               })
           .ToListAsync(ct);

        return new PagedResult<SaleKpiTargetDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            Items = items
        };
    }

    // =========================================================
    // KPI COMMISSIONS CALCULATION
    // =========================================================
    public async Task<List<KpiCommissionDto>> CalculateMonthlyKpiAsync(CalculateMonthlyKpiDto dto, CancellationToken ct = default)
    {
        ValidateYearMonth(dto.Year, dto.Month);

        var results = new List<KpiCommissionDto>();

        var salesQuery = _db.Users
     .Include(u => u.UserRoles)
       .ThenInclude(ur => ur.Role)
         .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == RoleName.Sale));

        if (dto.SaleId.HasValue)
            salesQuery = salesQuery.Where(u => u.UserId == dto.SaleId.Value);

        var sales = await salesQuery.ToListAsync(ct);

        foreach (var sale in sales)
        {
            var result = await CalculateSingleSaleKpiAsync(sale.UserId, dto.Year, dto.Month, ct);
            if (result != null)
                results.Add(result);
        }

        return results;
    }

    private async Task<KpiCommissionDto?> CalculateSingleSaleKpiAsync(int saleId, int year, int month, CancellationToken ct)
    {
        // Orders of sale in target month (Paid only)
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var paidOrders = await _db.Orders
   .AsNoTracking()
            .Where(o => o.SaleId == saleId)
            .Where(o => o.Status == OrderStatus.Paid)
     .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(ct);

        if (paidOrders.Count == 0)
            return null;

        var totalRevenue = paidOrders.Sum(o => o.TotalPrice);
        var baseCommission = totalRevenue * DEFAULT_COMMISSION_PERCENT / 100;

        // Simplest-realistic rule:
        // - if no monthly target => no KPI bonus
        // - if revenue < target => no KPI bonus
        // - if achieved >= target => choose tier by achieved percent, then apply BonusPercent on revenue
        var target = await _db.SaleKpiTargets
 .SingleOrDefaultAsync(x => x.SaleId == saleId && x.Year == year && x.Month == month, ct);

        int? achievedTierId = null;
        string? achievedTierName = null;
        int bonusCommission = 0;

        if (target != null && target.TargetRevenue > 0 && totalRevenue >= target.TargetRevenue)
        {
            var achievedPct = (int)Math.Floor(totalRevenue * 100.0 / target.TargetRevenue);

            // NOTE: KpiTier.MinRevenue is interpreted as MinAchievedPercent
            var tier = await _db.KpiTiers
 .AsNoTracking()
 .Where(t => t.MinRevenue <= achievedPct)
 .OrderByDescending(t => t.MinRevenue)
 .ThenBy(t => t.DisplayOrder)
 .FirstOrDefaultAsync(ct);

            if (tier != null)
            {
                achievedTierId = tier.KpiTierId;
                achievedTierName = tier.Name;
                bonusCommission = totalRevenue * Math.Clamp(tier.BonusPercent, 0, 100) / 100;
            }
        }

        var totalCommission = baseCommission + bonusCommission;

        var kpiComm = await _db.KpiCommissions
 .SingleOrDefaultAsync(x => x.SaleId == saleId && x.Year == year && x.Month == month, ct);

        if (kpiComm == null)
        {
            kpiComm = new KpiCommission
            {
                SaleId = saleId,
                Year = year,
                Month = month,
                BaseCommission = baseCommission,
                BonusCommission = bonusCommission,
                TotalCommission = totalCommission,
                KpiTierId = achievedTierId,
                TotalRevenue = totalRevenue,
                TotalOrders = paidOrders.Count,
                CalculatedAt = DateTime.UtcNow
            };
            _db.KpiCommissions.Add(kpiComm);
        }
        else
        {
            kpiComm.BaseCommission = baseCommission;
            kpiComm.BonusCommission = bonusCommission;
            kpiComm.TotalCommission = totalCommission;
            kpiComm.KpiTierId = achievedTierId;
            kpiComm.TotalRevenue = totalRevenue;
            kpiComm.TotalOrders = paidOrders.Count;
            kpiComm.CalculatedAt = DateTime.UtcNow;
        }

        // Update target snapshot (optional)
        if (target != null)
        {
            target.ActualRevenue = totalRevenue;
            target.KpiTierId = achievedTierId;
            target.BonusAmount = bonusCommission;
            target.CalculatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        var sale = await _db.Users.FindAsync(new object[] { saleId }, ct);

        return new KpiCommissionDto
        {
            KpiCommissionId = kpiComm.KpiCommissionId,
            SaleId = saleId,
            SaleName = sale?.FullName,
            Year = year,
            Month = month,
            BaseCommission = baseCommission,
            BonusCommission = bonusCommission,
            TotalCommission = totalCommission,
            KpiTierId = achievedTierId,
            KpiTierName = achievedTierName,
            TotalRevenue = totalRevenue,
            TotalOrders = paidOrders.Count,
            CalculatedAt = kpiComm.CalculatedAt
        };
    }

    public async Task<PagedResult<KpiCommissionDto>> GetKpiCommissionsAsync(
        int? saleId,
        int? year,
        int? month,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            throw new Exception("Invalid year.");
        if (month.HasValue && (month.Value < 1 || month.Value > 12))
            throw new Exception("Invalid month (1..12).");

        var query = _db.KpiCommissions
       .AsNoTracking()
            .Include(x => x.Sale)
       .Include(x => x.KpiTier)
 .AsQueryable();

        if (saleId.HasValue)
            query = query.Where(x => x.SaleId == saleId.Value);
        if (year.HasValue)
            query = query.Where(x => x.Year == year.Value);
        if (month.HasValue)
            query = query.Where(x => x.Month == month.Value);

        query = query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

        var total = await query.CountAsync(ct);

        var items = await query
       .Skip((page - 1) * pageSize)
                 .Take(pageSize)
        .Select(x => new KpiCommissionDto
        {
            KpiCommissionId = x.KpiCommissionId,
            SaleId = x.SaleId,
            SaleName = x.Sale.FullName,
            Year = x.Year,
            Month = x.Month,
            BaseCommission = x.BaseCommission,
            BonusCommission = x.BonusCommission,
            TotalCommission = x.TotalCommission,
            KpiTierId = x.KpiTierId,
            KpiTierName = x.KpiTier != null ? x.KpiTier.Name : null,
            TotalRevenue = x.TotalRevenue,
            TotalOrders = x.TotalOrders,
            CalculatedAt = x.CalculatedAt
        })
           .ToListAsync(ct);

        return new PagedResult<KpiCommissionDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            Items = items
        };
    }

    public async Task<KpiCommissionDto?> GetKpiCommissionAsync(int saleId, int year, int month, CancellationToken ct = default)
    {
        ValidateYearMonth(year, month);

        return await _db.KpiCommissions
                 .AsNoTracking()
                 .Include(x => x.Sale)
                   .Include(x => x.KpiTier)
       .Where(x => x.SaleId == saleId && x.Year == year && x.Month == month)
        .Select(x => new KpiCommissionDto
        {
            KpiCommissionId = x.KpiCommissionId,
            SaleId = x.SaleId,
            SaleName = x.Sale.FullName,
            Year = x.Year,
            Month = x.Month,
            BaseCommission = x.BaseCommission,
            BonusCommission = x.BonusCommission,
            TotalCommission = x.TotalCommission,
            KpiTierId = x.KpiTierId,
            KpiTierName = x.KpiTier != null ? x.KpiTier.Name : null,
            TotalRevenue = x.TotalRevenue,
            TotalOrders = x.TotalOrders,
            CalculatedAt = x.CalculatedAt
        })
                   .SingleOrDefaultAsync(ct);
    }

    // =========================================================
    // DASHBOARD
    // =========================================================
    public async Task<KpiDashboardDto> GetKpiDashboardAsync(int saleId, int? year = null, int? month = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;
        ValidateYearMonth(targetYear, targetMonth);

        var sale = await _db.Users.FindAsync(new object[] { saleId }, ct);
        if (sale == null)
            throw new Exception($"Sale {saleId} not found.");

        var target = await _db.SaleKpiTargets
 .AsNoTracking()
 .Include(x => x.KpiTier)
 .SingleOrDefaultAsync(x => x.SaleId == saleId && x.Year == targetYear && x.Month == targetMonth, ct);

        var startDate = new DateTime(targetYear, targetMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var orders = await _db.Orders
            .AsNoTracking()
               .Where(o => o.SaleId == saleId)
     .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
           .ToListAsync(ct);

        var paidOrders = orders.Where(o => o.Status == OrderStatus.Paid).ToList();
        var actualRevenue = paidOrders.Sum(o => o.TotalPrice);

        var estimatedBaseCommission = actualRevenue * DEFAULT_COMMISSION_PERCENT / 100;

        // Available tiers (MinRevenue interpreted as MinAchievedPercent)
        var availableTiers = await _db.KpiTiers
 .AsNoTracking()
 .OrderBy(x => x.DisplayOrder)
 .Select(x => new KpiTierDto
 {
     KpiTierId = x.KpiTierId,
     Name = x.Name,
     MinRevenue = x.MinRevenue,
     BonusPercent = x.BonusPercent,
     Description = x.Description,
     DisplayOrder = x.DisplayOrder
 })
          .ToListAsync(ct);

        int estimatedBonusCommission = 0;
        int? currentTierId = null;
        string? currentTierName = null;

        if (target != null && target.TargetRevenue > 0 && actualRevenue >= target.TargetRevenue)
        {
            var achievedPct = (int)Math.Floor(actualRevenue * 100.0 / target.TargetRevenue);

            var tier = availableTiers
 .Where(t => t.MinRevenue <= achievedPct)
 .OrderByDescending(t => t.MinRevenue)
 .ThenBy(t => t.DisplayOrder)
 .FirstOrDefault();

            if (tier != null)
            {
                currentTierId = tier.KpiTierId;
                currentTierName = tier.Name;
                estimatedBonusCommission = actualRevenue * Math.Clamp(tier.BonusPercent, 0, 100) / 100;
            }
        }

        return new KpiDashboardDto
        {
            SaleId = saleId,
            SaleName = sale.FullName ?? sale.Username,
            CurrentYear = targetYear,
            CurrentMonth = targetMonth,
            TargetRevenue = target?.TargetRevenue ?? 0,
            ActualRevenue = actualRevenue,
            EstimatedBaseCommission = estimatedBaseCommission,
            EstimatedBonusCommission = estimatedBonusCommission,
            EstimatedTotalCommission = estimatedBaseCommission + estimatedBonusCommission,
            CurrentKpiTierId = currentTierId,
            CurrentKpiTierName = currentTierName,
            TotalOrdersThisMonth = orders.Count,
            TotalOrdersPaid = paidOrders.Count,
            AvailableTiers = availableTiers
        };
    }
}
