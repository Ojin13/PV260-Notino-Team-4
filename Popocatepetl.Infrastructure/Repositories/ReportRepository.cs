using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

/// <summary>EF Core implementation of IReportRepository.</summary>
public sealed class ReportRepository(PopocatepetlDbContext context) : IReportRepository
{
    public async Task<Report?> GetLatestAsync() =>
        await context.Reports
            .AsNoTracking()
            .OrderByDescending(r => r.UploadedAt)
            .FirstOrDefaultAsync(r => r.IsLatest);

    public async Task<Report?> GetByIdAsync(Guid id) =>
        await context.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IReadOnlyList<Report>> GetAllAsync() =>
        await context.Reports
            .AsNoTracking()
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();

    public async Task SaveAsync(Report report)
    {
        var exists = await context.Reports.AnyAsync(r => r.Id == report.Id);
        if (exists)
        {
            context.Reports.Update(report);
        }
        else
        {
            await context.Reports.AddAsync(report);
        }

        await context.SaveChangesAsync();
    }

    public async Task SetLatestAsync(Guid reportId)
    {
        await context.Reports
            .Where(r => r.IsLatest)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.IsLatest, false));

        await context.Reports
            .Where(r => r.Id == reportId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.IsLatest, true));
    }
}
