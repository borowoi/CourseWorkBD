using Microsoft.EntityFrameworkCore;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

public sealed class LookupService(IUnitOfWork uow) : ILookupService
{
    public async Task<IReadOnlyList<LookupDto>> GetClassesAsync(string? academicYear = null, CancellationToken ct = default)
    {
        var q = uow.Classes.Query();
        if (!string.IsNullOrWhiteSpace(academicYear))
            q = q.Where(c => c.AcademicYear == academicYear);

        var list = await q
            .OrderBy(c => c.GradeLevel).ThenBy(c => c.ClassLetter)
            .Select(c => new { c.ClassId, c.GradeLevel, c.ClassLetter })
            .ToListAsync(ct);

        return list.Select(c => new LookupDto(c.ClassId, $"{c.GradeLevel}{c.ClassLetter}")).ToList();
    }

    public async Task<IReadOnlyList<LookupDto>> GetSubjectsAsync(CancellationToken ct = default)
    {
        var list = await uow.Subjects.Query()
            .OrderBy(s => s.SubjectName)
            .Select(s => new { s.SubjectId, s.SubjectName })
            .ToListAsync(ct);
        return list.Select(s => new LookupDto(s.SubjectId, s.SubjectName)).ToList();
    }

    public async Task<IReadOnlyList<LookupDto>> GetTeachersAsync(CancellationToken ct = default)
    {
        var list = await uow.Teachers.Query()
            .OrderBy(t => t.FullName)
            .Select(t => new { t.TeacherId, t.FullName })
            .ToListAsync(ct);
        return list.Select(t => new LookupDto(t.TeacherId, t.FullName)).ToList();
    }

    public async Task<IReadOnlyList<LookupDto>> GetStudentsAsync(int classId, CancellationToken ct = default)
    {
        var list = await uow.Students.Query()
            .Where(s => s.CurrentClassId == classId)
            .OrderBy(s => s.FullName)
            .Select(s => new { s.StudentId, s.FullName })
            .ToListAsync(ct);
        return list.Select(s => new LookupDto(s.StudentId, s.FullName)).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAcademicYearsAsync(CancellationToken ct = default)
        => await uow.Classes.Query()
            .Select(c => c.AcademicYear)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync(ct);
}
