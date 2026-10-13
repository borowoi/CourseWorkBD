using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

/// <summary>Отметки и пропуски учащегося за выбранный период.</summary>
public sealed class StudentReportService(IUnitOfWork uow) : IStudentReportService
{
    public async Task<StudentPeriodReportDto?> GetAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (from > to)
            throw new ArgumentException("Дата начала периода позже даты окончания.");

        var student = await JournalQueries.GetStudentAsync(uow, studentId, ct);
        if (student is null) return null;

        var entries = await JournalQueries.GetEntriesAsync(
            uow, new JournalEntryFilter { StudentId = studentId, From = from, To = to }, ct);

        var marks = entries.Where(e => e.GradeValue.HasValue)
            .OrderBy(e => e.LessonDate).ThenBy(e => e.LessonNumber)
            .Select(e => new StudentMark(e.LessonDate, e.LessonNumber, e.SubjectName, e.GradeValue!.Value, e.WorkType))
            .ToList();

        var absences = entries.Where(e => !e.IsPresent)
            .OrderBy(e => e.LessonDate).ThenBy(e => e.LessonNumber)
            .Select(e => new StudentAbsence(e.LessonDate, e.LessonNumber, e.SubjectName, e.AbsenceReason ?? "—"))
            .ToList();

        var averages = entries.Where(e => e.GradeValue.HasValue)
            .GroupBy(e => (e.SubjectId, e.SubjectName))
            .Select(g =>
            {
                var grades = g.Select(e => e.GradeValue!.Value).ToList();
                return new StudentSubjectAverage(
                    g.Key.SubjectId, g.Key.SubjectName, grades.Count,
                    GradeCalculator.Average(grades)!.Value,
                    GradeCalculator.FinalGrade(grades)!.Value);
            })
            .OrderBy(a => a.SubjectName)
            .ToList();

        var finals = await JournalQueries.GetFinalGradesAsync(uow, studentId, ct);

        return new StudentPeriodReportDto(
            student, from, to, marks, absences, averages, finals,
            GradeCalculator.Average(marks.Select(m => m.Grade)));
    }
}
