namespace Popocatepetl.Domain.Entities;

public class DiffResult : BaseEntity
{
    public Guid BaselineReportId { get; init; }
    public Guid CurrentReportId { get; init; }
    public Report BaselineReport { get; init; } = null!;
    public Report CurrentReport { get; init; } = null!;
    public IEnumerable<DiffData> DiffDataEntries { get; init; } = [];
    public DateTime GeneratedAt { get; init; }

    private DiffResult() { }

    public static DiffResult Create(Guid baselineReportId, Guid currentReportId, DateTime? generatedAt = null, IEnumerable<DiffData>? diffDataEntries = null) =>
        new DiffResult
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            BaselineReportId = baselineReportId,
            CurrentReportId = currentReportId,
            GeneratedAt = generatedAt ?? DateTime.UtcNow,
            DiffDataEntries = diffDataEntries ?? []
        };
}
