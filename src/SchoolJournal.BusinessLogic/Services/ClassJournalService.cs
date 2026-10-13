using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

/// <summary>Журнал класса по предмету за четверть со средним баллом каждого учащегося.</summary>
public sealed class ClassJournalService(IUnitOfWork uow) : IClassJournalService
{
    public async Task<ClassJournalDto?> GetAsync(int classId, int subjectId, int quarter, CancellationToken ct = default)
    {
        var cls = await JournalQueries.GetClassAsync(uow, classId, ct);
        var subject = await JournalQueries.GetSubjectAsync(uow, subjectId, ct);
        if (cls is null || subject is null) return null;

        var (from, to) = AcademicCalendar.GetQuarterRange(cls.AcademicYear, quarter);
        var students = await JournalQueries.GetStudentsByClassAsync(uow, classId, ct);
        var entries = await JournalQueries.GetEntriesAsync(
            uow, new JournalEntryFilter { ClassId = classId, SubjectId = subjectId, From = from, To = to }, ct);

        var lessons = entries
            .GroupBy(e => e.LessonId).Select(g => g.First())
            .OrderBy(e => e.LessonDate).ThenBy(e => e.LessonNumber)
            .Select(e => new LessonColumn(e.LessonId, e.LessonDate, e.LessonNumber))
            .ToList();

        var byStudent = entries.ToLookup(e => e.StudentId);
        var rows = students.OrderBy(s => s.FullName).Select(s =>
        {
            var own = byStudent[s.StudentId].ToList();
            var grades = own.Where(e => e.GradeValue.HasValue).Select(e => e.GradeValue!.Value).ToList();
            var cells = own.GroupBy(e => e.LessonId).ToDictionary(
                g => g.Key,
                g => new JournalCell(g.First().GradeValue, g.First().IsPresent,
                                     g.First().AbsenceReason, g.First().WorkType));
            return new ClassJournalRow(
                s.StudentId, s.FullName,
                cells,
                GradeCalculator.Average(grades),
                GradeCalculator.FinalGrade(grades),
                own.Count(e => !e.IsPresent));
        }).ToList();

        return new ClassJournalDto(cls, subject, quarter, from, to, lessons, rows);
    }
}
