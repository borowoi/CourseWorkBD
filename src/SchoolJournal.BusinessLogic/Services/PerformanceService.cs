using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

/// <summary>Отчёт по успеваемости класса и параллели: средняя оценка, неуспевающие, рейтинг предметов.</summary>
public sealed class PerformanceService(IUnitOfWork uow) : IPerformanceService
{
    public async Task<PerformanceReportDto?> GetForClassAsync(int classId, int quarter, CancellationToken ct = default)
    {
        var cls = await JournalQueries.GetClassAsync(uow, classId, ct);
        if (cls is null) return null;

        var (from, to) = AcademicCalendar.GetQuarterRange(cls.AcademicYear, quarter);
        var entries = await JournalQueries.GetEntriesAsync(
            uow, new JournalEntryFilter { ClassId = classId, From = from, To = to, OnlyGraded = true }, ct);

        return Build($"Класс {cls.Name}", cls.AcademicYear, quarter, entries);
    }

    public async Task<PerformanceReportDto> GetForParallelAsync(int gradeLevel, string academicYear, int quarter, CancellationToken ct = default)
    {
        var (from, to) = AcademicCalendar.GetQuarterRange(academicYear, quarter);
        var entries = await JournalQueries.GetEntriesAsync(
            uow,
            new JournalEntryFilter
            {
                GradeLevel = gradeLevel, AcademicYear = academicYear,
                From = from, To = to, OnlyGraded = true
            }, ct);

        return Build($"Параллель {gradeLevel}", academicYear, quarter, entries);
    }

    /// <summary>Чистая функция расчёта: покрывается модульными тестами без базы данных.</summary>
    public static PerformanceReportDto Build(
        string title, string academicYear, int quarter, IReadOnlyList<JournalEntryRecord> entries)
    {
        var graded = entries.Where(e => e.GradeValue.HasValue).ToList();

        var finals = graded
            .GroupBy(e => new { e.StudentId, e.SubjectId, e.ClassId })
            .Select(g => new
            {
                g.Key.StudentId, g.Key.SubjectId, g.Key.ClassId,
                Final = GradeCalculator.FinalGrade(g.Select(e => e.GradeValue!.Value))!.Value
            })
            .ToList();

        var subjects = graded
            .GroupBy(e => new { e.SubjectId, e.SubjectName })
            .Select(g =>
            {
                var f = finals.Where(x => x.SubjectId == g.Key.SubjectId).ToList();
                return new
                {
                    g.Key.SubjectId, g.Key.SubjectName,
                    Students = f.Count,
                    Average = GradeCalculator.Average(g.Select(e => e.GradeValue!.Value)),
                    Failing = f.Count(x => GradeCalculator.IsFailing(x.Final))
                };
            })
            .OrderByDescending(s => s.Average).ThenBy(s => s.SubjectName)
            .Select((s, i) => new SubjectPerformance(
                i + 1, s.SubjectId, s.SubjectName, s.Students, s.Average, s.Failing))
            .ToList();

        var classes = graded
            .GroupBy(e => new { e.ClassId, e.GradeLevel, e.ClassLetter })
            .OrderBy(g => g.Key.GradeLevel).ThenBy(g => g.Key.ClassLetter)
            .Select(g => new ClassPerformance(
                $"{g.Key.GradeLevel}{g.Key.ClassLetter}",
                g.Select(e => e.StudentId).Distinct().Count(),
                GradeCalculator.Average(g.Select(e => e.GradeValue!.Value)),
                finals.Where(x => x.ClassId == g.Key.ClassId && GradeCalculator.IsFailing(x.Final))
                      .Select(x => x.StudentId).Distinct().Count()))
            .ToList();

        var failingStudents = finals.Where(x => GradeCalculator.IsFailing(x.Final))
                                    .Select(x => x.StudentId).Distinct().Count();

        return new PerformanceReportDto(
            title, academicYear, quarter,
            GradeCalculator.Average(graded.Select(e => e.GradeValue!.Value)),
            failingStudents, subjects, classes);
    }
}
