using Microsoft.EntityFrameworkCore;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

public sealed class StatisticsService(IUnitOfWork uow) : IStatisticsService
{
    public async Task<DashboardDto> GetAsync(CancellationToken ct = default)
    {
        var teachers = await uow.Teachers.Query().CountAsync(ct);
        var subjects = await uow.Subjects.Query().CountAsync(ct);
        var classes = await uow.Classes.Query().CountAsync(ct);
        var students = await uow.Students.Query().CountAsync(ct);
        var lessons = await uow.Lessons.Query().CountAsync(ct);
        var entries = await uow.Journals.Query().CountAsync(ct);
        return new DashboardDto(teachers, subjects, classes, students, lessons, entries);
    }
}
