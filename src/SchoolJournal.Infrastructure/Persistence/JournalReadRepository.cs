using Microsoft.EntityFrameworkCore;
using SchoolJournal.Application.Abstractions;
using SchoolJournal.Application.Dtos;

namespace SchoolJournal.Infrastructure.Persistence;

public sealed class JournalReadRepository(SchoolJournalDbContext db) : IJournalReadRepository
{
    public async Task<ClassInfo?> GetClassAsync(int classId, CancellationToken ct = default) =>
        await db.Classes.AsNoTracking()
            .Where(c => c.ClassId == classId)
            .Select(c => new ClassInfo(c.ClassId, c.GradeLevel, c.ClassLetter, c.AcademicYear))
            .FirstOrDefaultAsync(ct);

    public async Task<SubjectInfo?> GetSubjectAsync(int subjectId, CancellationToken ct = default) =>
        await db.Subjects.AsNoTracking()
            .Where(s => s.SubjectId == subjectId)
            .Select(s => new SubjectInfo(s.SubjectId, s.SubjectName))
            .FirstOrDefaultAsync(ct);

    public async Task<StudentInfo?> GetStudentAsync(int studentId, CancellationToken ct = default)
    {
        var s = await db.Students.AsNoTracking()
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

    public async Task<IReadOnlyList<StudentInfo>> GetStudentsByClassAsync(int classId, CancellationToken ct = default)
    {
        var list = await db.Students.AsNoTracking()
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

    public async Task<IReadOnlyList<JournalEntryRecord>> GetEntriesAsync(
        JournalEntryFilter filter, CancellationToken ct = default)
    {
        var q = db.Journals.AsNoTracking().AsQueryable();

        if (filter.ClassId.HasValue)      q = q.Where(j => j.Lesson.ClassId == filter.ClassId.Value);
        if (filter.GradeLevel.HasValue)   q = q.Where(j => j.Lesson.Class.GradeLevel == filter.GradeLevel.Value);
        if (!string.IsNullOrEmpty(filter.AcademicYear))
                                          q = q.Where(j => j.Lesson.Class.AcademicYear == filter.AcademicYear);
        if (filter.StudentId.HasValue)    q = q.Where(j => j.StudentId == filter.StudentId.Value);
        if (filter.SubjectId.HasValue)    q = q.Where(j => j.Lesson.SubjectId == filter.SubjectId.Value);
        if (filter.From.HasValue)         q = q.Where(j => j.Lesson.LessonDate >= filter.From.Value);
        if (filter.To.HasValue)           q = q.Where(j => j.Lesson.LessonDate <= filter.To.Value);
        if (filter.OnlyGraded)            q = q.Where(j => j.GradeValue != null);
        if (filter.OnlyAbsences)          q = q.Where(j => !j.IsPresent);

        return await q
            .Select(j => new JournalEntryRecord(
                j.JournalId, j.StudentId, j.Student.FullName,
                j.Lesson.ClassId, j.Lesson.Class.GradeLevel, j.Lesson.Class.ClassLetter,
                j.Lesson.SubjectId, j.Lesson.Subject.SubjectName,
                j.LessonId, j.Lesson.LessonDate, j.Lesson.LessonNumber,
                j.IsPresent, j.AbsenceReason, j.GradeValue, j.WorkType))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FinalGradeRecord>> GetFinalGradesAsync(int studentId, CancellationToken ct = default)
    {
        var rows = await db.VFinalGrades.AsNoTracking()
            .Where(f => f.StudentId == studentId)
            .ToListAsync(ct);

        return rows
            .OrderBy(f => f.SubjectName).ThenBy(f => f.PeriodType)
            .Select(f => new FinalGradeRecord(
                f.StudentId, f.SubjectId, f.SubjectName, f.AcademicYear, f.PeriodType,
                f.FinalGrade.HasValue ? (int)Math.Round(f.FinalGrade.Value, MidpointRounding.AwayFromZero) : null))
            .ToList();
    }
}
