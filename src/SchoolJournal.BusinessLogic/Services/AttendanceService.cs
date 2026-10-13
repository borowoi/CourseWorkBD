using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

/// <summary>Отчёт по посещаемости: пропущенные уроки по классам и предметам за период.</summary>
public sealed class AttendanceService(IUnitOfWork uow) : IAttendanceService
{
    public async Task<AttendanceReportDto> GetAsync(
        DateOnly from, DateOnly to, int? classId = null, int? subjectId = null, CancellationToken ct = default)
    {
        if (from > to)
            throw new ArgumentException("Дата начала периода позже даты окончания.");

        var entries = await JournalQueries.GetEntriesAsync(
            uow,
            new JournalEntryFilter
            {
                ClassId = classId, SubjectId = subjectId,
                From = from, To = to, OnlyAbsences = true
            }, ct);

        return Build(from, to, entries);
    }

    public static AttendanceReportDto Build(DateOnly from, DateOnly to, IReadOnlyList<JournalEntryRecord> entries)
    {
        var rows = entries
            .Where(e => !e.IsPresent)
            .GroupBy(e => new { e.GradeLevel, e.ClassLetter, e.SubjectName })
            .OrderBy(g => g.Key.GradeLevel).ThenBy(g => g.Key.ClassLetter).ThenBy(g => g.Key.SubjectName)
            .Select(g => new AttendanceRow(
                $"{g.Key.GradeLevel}{g.Key.ClassLetter}", g.Key.SubjectName,
                g.Count(e => e.AbsenceReason == AbsenceReasons.Illness),
                g.Count(e => e.AbsenceReason == AbsenceReasons.Valid),
                g.Count(e => e.AbsenceReason == AbsenceReasons.Unexcused),
                g.Count()))
            .ToList();

        return new AttendanceReportDto(from, to, rows, rows.Sum(r => r.Total));
    }
}
