using Application.Common.Interfaces.Repositories;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Data.Repositories;

public class ReportRepository : BaseRepository<Report>, IReportRepository
{
    private readonly DbSet<Report> _reports;

    public ReportRepository(ApplicationDbContext context) : base(context, context.Reports)
    {
        _reports = context.Reports;
    }

    public override async Task<Report> GetByIdAsync(Guid id)
    {
        return await _reports
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id) ?? throw new EntityNotFoundException<Report>();
    }

    public async Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize)
    {
        return await _reports
            .Where(r => r.Status != ReportStatus.Resolved)
            .OrderByDescending(r => r.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}