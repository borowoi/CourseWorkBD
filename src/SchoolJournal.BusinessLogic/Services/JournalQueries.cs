using Microsoft.EntityFrameworkCore;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

/// <summary>Общие выборки для служб отчётов. Фильтрация выполняется на стороне БД.</summary>
internal static class JournalQueries
{
    public static Task<ClassInfo?> GetClassAsync(IUnitOfWork uow, int classId, CancellationToken ct) =>
        uow.Classes.Query()
            .Where(c => c.ClassId == classId)
            .Select(c => new ClassInfo(c.ClassId, c.GradeLevel, c.ClassLetter, c.AcademicYear))
            .FirstOrDefaultAsync(ct);

    public static Task<SubjectInfo?> GetSubjectAsync(IUnitOfWork uow, int subjectId, CancellationToken ct) =>
        uow.Subjects.Query()
            .Where(s => s.SubjectId == subjectId)
            .Select(s => new SubjectInfo(s.SubjectId, s.SubjectName))
            .FirstOrDefaultAsync(ct);

    public static async Task<StudentInfo?> GetStudentAsync(IUnitOfWork uow, int studentId, CancellationToken ct)
    {
        var s = await uow.Students.Query()
            .Where(x => x.StudentId == studentId)
            .Select(x => new
            {
                x.StudentId, x.FullName, x.CurrentClassId,
                x.CurrentClass.GradeLevel, x.CurrentClass.ClassLetter
            })
            .FirstOrDefaultAsync(ct);

        return s is null
            ? null
            : new StudentInfo(s.StudentId, s.FullName, s.CurrentClassId, $"{s.GradeLevel}{s.ClassLetter}");
    }

    public static async Task<IReadOnlyList<StudentInfo>> GetStudentsByClassAsync(
        IUnitOfWork uow, int classId, CancellationToken ct)
    {
        var list = await uow.Students.Query()
            .Where(x => x.CurrentClassId == classId)
            .Select(x => new
            {
                x.StudentId, x.FullName, x.CurrentClassId,
                x.CurrentClass.GradeLevel, x.CurrentClass.ClassLetter
            })
            .ToListAsync(ct);

        return list
            .Select(s => new StudentInfo(s.StudentId, s.FullName, s.CurrentClassId, $"{s.GradeLevel}{s.ClassLetter}"))
            .ToList();
    }

    public static async Task<IReadOnlyList<JournalEntryRecord>> GetEntriesAsync(
        IUnitOfWork uow, JournalEntryFilter filter, CancellationToken ct)
    {
        var q = uow.Journals.Query();

        if (filter.ClassId.HasValue)    q = q.Where(j => j.Lesson.ClassId == filter.ClassId.Value);
        if (filter.GradeLevel.HasValue) q = q.Where(j => j.Lesson.Class.GradeLevel == filter.GradeLevel.Value);
        if (!string.IsNullOrEmpty(filter.AcademicYear))
                                        q = q.Where(j => j.Lesson.Class.AcademicYear == filter.AcademicYear);
        if (filter.StudentId.HasValue)  q = q.Where(j => j.StudentId == filter.StudentId.Value);
        if (filter.SubjectId.HasValue)  q = q.Where(j => j.Lesson.SubjectId == filter.SubjectId.Value);
        if (filter.From.HasValue)       q = q.Where(j => j.Lesson.LessonDate >= filter.From.Value);
        if (filter.To.HasValue)         q = q.Where(j => j.Lesson.LessonDate <= filter.To.Value);
        if (filter.OnlyGraded)          q = q.Where(j => j.GradeValue != null);
        if (filter.OnlyAbsences)        q = q.Where(j => !j.IsPresent);

        return await q
            .Select(j => new JournalEntryRecord(
                j.JournalId, j.StudentId, j.Student.FullName,
                j.Lesson.ClassId, j.Lesson.Class.GradeLevel, j.Lesson.Class.ClassLetter,
                j.Lesson.SubjectId, j.Lesson.Subject.SubjectName,
                j.LessonId, j.Lesson.LessonDate, j.Lesson.LessonNumber,
                j.IsPresent, j.AbsenceReason, j.GradeValue, j.WorkType))
            .ToListAsync(ct);
    }

    public static async Task<IReadOnlyList<FinalGradeRecord>> GetFinalGradesAsync(
        IUnitOfWork uow, int studentId, CancellationToken ct)
    {
        var rows = await uow.FinalGrades.Query()
            .Where(f => f.StudentId == studentId)
            .ToListAsync(ct);

        return rows
            .OrderBy(f => f.SubjectName).ThenBy(f => f.PeriodType)
            .Select(f => new FinalGradeRecord(
                f.StudentId, f.SubjectId, f.SubjectName, f.AcademicYear, f.PeriodType ?? string.Empty,
                f.FinalGrade.HasValue ? (int)Math.Round(f.FinalGrade.Value, MidpointRounding.AwayFromZero) : null))
            .ToList();
    }
}
