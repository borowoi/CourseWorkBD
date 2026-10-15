using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

public sealed class ClassService(IUnitOfWork uow) : IClassService
{
    public Task<PagedResult<ClassDto>> GetPagedAsync(ClassFilterDto filter, CancellationToken ct = default)
    {
        var q = uow.Classes.Query();

        if (filter.GradeLevel.HasValue)
        {
            var grade = filter.GradeLevel.Value;
            q = q.Where(c => c.GradeLevel == grade);
        }
        if (!string.IsNullOrWhiteSpace(filter.AcademicYear))
        {
            var year = filter.AcademicYear;
            q = q.Where(c => c.AcademicYear == year);
        }
        if (filter.HomeroomTeacherId.HasValue)
        {
            var teacherId = filter.HomeroomTeacherId.Value;
            q = q.Where(c => c.HomeroomTeacherId == teacherId);
        }

        q = filter.SortOrder switch
        {
            "grade_desc" => q.OrderByDescending(c => c.GradeLevel).ThenBy(c => c.ClassLetter),
            "year" => q.OrderBy(c => c.AcademicYear).ThenBy(c => c.GradeLevel).ThenBy(c => c.ClassLetter),
            "year_desc" => q.OrderByDescending(c => c.AcademicYear).ThenBy(c => c.GradeLevel).ThenBy(c => c.ClassLetter),
            "teacher" => q.OrderBy(c => c.HomeroomTeacher!.FullName),
            _ => q.OrderBy(c => c.GradeLevel).ThenBy(c => c.ClassLetter)
        };

        return q.ToPagedAsync(filter, c => new ClassDto
        {
            Id = c.ClassId,
            GradeLevel = c.GradeLevel,
            ClassLetter = c.ClassLetter,
            AcademicYear = c.AcademicYear,
            HomeroomTeacherId = c.HomeroomTeacherId,
            HomeroomTeacherName = c.HomeroomTeacher != null ? c.HomeroomTeacher.FullName : null,
            StudentsCount = c.Students.Count()
        }, ct);
    }

    public async Task<ClassEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var c = await uow.Classes.GetByIdAsync(id, ct);
        return c is null
            ? null
            : new ClassEditDto
            {
                Id = c.ClassId,
                GradeLevel = c.GradeLevel,
                ClassLetter = c.ClassLetter,
                HomeroomTeacherId = c.HomeroomTeacherId,
                AcademicYear = c.AcademicYear
            };
    }

    public async Task<int> CreateAsync(ClassEditDto dto, CancellationToken ct = default)
    {
        var letter = await ValidateAsync(dto, ct);
        var entity = new Class
        {
            GradeLevel = dto.GradeLevel,
            ClassLetter = letter,
            HomeroomTeacherId = dto.HomeroomTeacherId,
            AcademicYear = dto.AcademicYear
        };
        await uow.Classes.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.ClassId;
    }

    public async Task UpdateAsync(ClassEditDto dto, CancellationToken ct = default)
    {
        var letter = await ValidateAsync(dto, ct);
        var entity = await uow.Classes.GetByIdAsync(dto.Id, ct)
            ?? throw new BusinessRuleException("Класс не найден.");
        entity.GradeLevel = dto.GradeLevel;
        entity.ClassLetter = letter;
        entity.HomeroomTeacherId = dto.HomeroomTeacherId;
        entity.AcademicYear = dto.AcademicYear;
        uow.Classes.Update(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Classes.GetByIdAsync(id, ct)
            ?? throw new BusinessRuleException("Класс не найден.");

        if (await uow.Students.ExistsAsync(s => s.CurrentClassId == id, ct))
            throw new BusinessRuleException("Удаление невозможно: в классе есть учащиеся.");
        if (await uow.Lessons.ExistsAsync(l => l.ClassId == id, ct))
            throw new BusinessRuleException("Удаление невозможно: для класса проведены уроки.");

        uow.Classes.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    /// <summary>Проверяет данные и возвращает нормализованную литеру.</summary>
    private async Task<string> ValidateAsync(ClassEditDto dto, CancellationToken ct)
    {
        if (dto.GradeLevel is < 1 or > 11)
            throw new BusinessRuleException("Параллель должна быть от 1 до 11.");

        var letter = (dto.ClassLetter ?? string.Empty).Trim().ToUpperInvariant();
        if (letter.Length != 1 || !char.IsLetter(letter[0]))
            throw new BusinessRuleException("Литера класса должна состоять из одной буквы.");

        var year = dto.AcademicYear ?? string.Empty;
        if (year.Length != 9 || year[4] != '/' ||
            !int.TryParse(year.AsSpan(0, 4), out var first) ||
            !int.TryParse(year.AsSpan(5, 4), out var second))
            throw new BusinessRuleException("Учебный год должен быть в формате 2026/2027.");
        if (second != first + 1)
            throw new BusinessRuleException("Учебный год должен состоять из двух последовательных лет, например 2026/2027.");

        if (dto.HomeroomTeacherId.HasValue)
        {
            var teacherId = dto.HomeroomTeacherId.Value;
            if (!await uow.Teachers.ExistsAsync(t => t.TeacherId == teacherId, ct))
                throw new BusinessRuleException("Классный руководитель не найден.");
        }

        var grade = dto.GradeLevel;
        var id = dto.Id;
        if (await uow.Classes.ExistsAsync(
                c => c.GradeLevel == grade && c.ClassLetter == letter && c.AcademicYear == year && c.ClassId != id, ct))
            throw new BusinessRuleException("Такой класс в указанном учебном году уже существует.");

        return letter;
    }
}
