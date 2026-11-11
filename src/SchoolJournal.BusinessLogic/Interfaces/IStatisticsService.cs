using SchoolJournal.BusinessLogic.DTOs;

namespace SchoolJournal.BusinessLogic.Interfaces;

public interface IStatisticsService
{
    Task<DashboardDto> GetAsync(CancellationToken ct = default);
}
