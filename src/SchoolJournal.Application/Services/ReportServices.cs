using SchoolJournal.Application.Abstractions;
using SchoolJournal.Application.Common;
using SchoolJournal.Application.Dtos;
using SchoolJournal.Domain.Constants;

namespace SchoolJournal.Application.Services;

// ===================== Журнал класса по предмету =====================
public interface IClassJournalService
{
    Task<ClassJournalDto?> GetAsync(int classId, int subjectId, int quarter, CancellationToken ct = default);
}

public sealed class ClassJournalService(IJournalReadRepository repo) : IClassJournalService
{
    public async Task<ClassJournalDto?> GetAsync(int classId, int subjectId, int quarter, CancellationToken ct = default)
    {
        var cls = await repo.GetClassAsync(classId, ct);
        var subject = await repo.GetSubjectAsync(subjectId, ct);
        if (cls is null || subject is null) return null;

        var (from, to) = AcademicCalendar.GetQuarterRange(cls.AcademicYear, quarter);
        var students = await repo.GetStudentsByClassAsync(classId, ct);
        var entries = await repo.GetEntriesAsync(
            new JournalEntryFilter { ClassId = classId, SubjectId = subjectId, From = from, To = to }, ct);

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

// ===================== Отметки и пропуски учащегося =====================
public interface IStudentReportService
{
    Task<StudentPeriodReportDto?> GetAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public sealed class StudentReportService(IJournalReadRepository repo) : IStudentReportService
{
    public async Task<StudentPeriodReportDto?> GetAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (from > to)
            throw new ArgumentException("Дата начала периода позже даты окончания.");

        var student = await repo.GetStudentAsync(studentId, ct);
        if (student is null) return null;

        var entries = await repo.GetEntriesAsync(
            new JournalEntryFilter { StudentId = studentId, From = from, To = to }, ct);

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

        var finals = await repo.GetFinalGradesAsync(studentId, ct);

        return new StudentPeriodReportDto(
            student, from, to, marks, absences, averages, finals,
            GradeCalculator.Average(marks.Select(m => m.Grade)));
    }
}

// ===================== Успеваемость класса / параллели =====================
public interface IPerformanceService
{
    Task<PerformanceReportDto?> GetForClassAsync(int classId, int quarter, CancellationToken ct = default);
    Task<PerformanceReportDto> GetForParallelAsync(int gradeLevel, string academicYear, int quarter, CancellationToken ct = default);
}

public sealed class PerformanceService(IJournalReadRepository repo) : IPerformanceService
{
    public async Task<PerformanceReportDto?> GetForClassAsync(int classId, int quarter, CancellationToken ct = default)
    {
        var cls = await repo.GetClassAsync(classId, ct);
        if (cls is null) return null;

        var (from, to) = AcademicCalendar.GetQuarterRange(cls.AcademicYear, quarter);
        var entries = await repo.GetEntriesAsync(
            new JournalEntryFilter { ClassId = classId, From = from, To = to, OnlyGraded = true }, ct);

        return Build($"Класс {cls.Name}", cls.AcademicYear, quarter, entries);
    }

    public async Task<PerformanceReportDto> GetForParallelAsync(int gradeLevel, string academicYear, int quarter, CancellationToken ct = default)
    {
        var (from, to) = AcademicCalendar.GetQuarterRange(academicYear, quarter);
        var entries = await repo.GetEntriesAsync(
            new JournalEntryFilter
            {
                GradeLevel = gradeLevel, AcademicYear = academicYear,
                From = from, To = to, OnlyGraded = true
            }, ct);

        return Build($"Параллель {gradeLevel}", academicYear, quarter, entries);
    }

    /// <summary>Чистая функция расчёта: удобно покрывать модульными тестами.</summary>
    public static PerformanceReportDto Build(
        string title, string academicYear, int quarter, IReadOnlyList<JournalEntryRecord> entries)
    {
        var graded = entries.Where(e => e.GradeValue.HasValue).ToList();

        // итоговая отметка каждого учащегося по каждому предмету
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

// ===================== Посещаемость =====================
public interface IAttendanceService
{
    Task<AttendanceReportDto> GetAsync(DateOnly from, DateOnly to, int? classId = null, int? subjectId = null, CancellationToken ct = default);
}

public sealed class AttendanceService(IJournalReadRepository repo) : IAttendanceService
{
    public async Task<AttendanceReportDto> GetAsync(
        DateOnly from, DateOnly to, int? classId = null, int? subjectId = null, CancellationToken ct = default)
    {
        if (from > to)
            throw new ArgumentException("Дата начала периода позже даты окончания.");

        var entries = await repo.GetEntriesAsync(
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
