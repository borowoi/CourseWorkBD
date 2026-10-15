using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

public sealed class StudentService(IUnitOfWork uow) : IStudentService
{
    /// <summary>Минимальный возраст учащегося на момент зачисления, лет.</summary>
    private const int MinEnrollmentAge = 5;

    public Task<PagedResult<StudentDto>> GetPagedAsync(StudentFilterDto filter, CancellationToken ct = default)
    {
        var q = uow.Students.Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            q = q.Where(s => s.FullName.Contains(search));
        }
        if (filter.ClassId.HasValue)
        {
            var classId = filter.ClassId.Value;
            q = q.Where(s => s.CurrentClassId == classId);
        }
        if (filter.GradeLevel.HasValue)
        {
            var grade = filter.GradeLevel.Value;
            q = q.Where(s => s.CurrentClass.GradeLevel == grade);
        }

        q = filter.SortOrder switch
        {
            "name_desc" => q.OrderByDescending(s => s.FullName),
            "class" => q.OrderBy(s => s.CurrentClass.GradeLevel).ThenBy(s => s.CurrentClass.ClassLetter).ThenBy(s => s.FullName),
            "class_desc" => q.OrderByDescending(s => s.CurrentClass.GradeLevel).ThenBy(s => s.CurrentClass.ClassLetter).ThenBy(s => s.FullName),
            "birth" => q.OrderBy(s => s.BirthDate),
            "birth_desc" => q.OrderByDescending(s => s.BirthDate),
            _ => q.OrderBy(s => s.FullName)
        };

        return q.ToPagedAsync(filter, s => new StudentDto
        {
            Id = s.StudentId,
            FullName = s.FullName,
            BirthDate = s.BirthDate,
            ClassId = s.CurrentClassId,
            GradeLevel = s.CurrentClass.GradeLevel,
            ClassLetter = s.CurrentClass.ClassLetter,
            ContactInfo = s.ContactInfo,
            EnrollmentDate = s.EnrollmentDate
        }, ct);
    }

    public async Task<StudentEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var s = await uow.Students.GetByIdAsync(id, ct);
        return s is null
            ? null
            : new StudentEditDto
            {
                Id = s.StudentId,
                FullName = s.FullName,
                BirthDate = s.BirthDate,
                CurrentClassId = s.CurrentClassId,
                ContactInfo = s.ContactInfo,
                EnrollmentDate = s.EnrollmentDate
            };
    }

    public async Task<int> CreateAsync(StudentEditDto dto, CancellationToken ct = default)
    {
        await ValidateAsync(dto, ct);
        var entity = new Student
        {
            FullName = dto.FullName.Trim(),
            BirthDate = dto.BirthDate,
            CurrentClassId = dto.CurrentClassId,
            ContactInfo = string.IsNullOrWhiteSpace(dto.ContactInfo) ? null : dto.ContactInfo.Trim(),
            EnrollmentDate = dto.EnrollmentDate
        };
        await uow.Students.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.StudentId;
    }

    public async Task UpdateAsync(StudentEditDto dto, CancellationToken ct = default)
    {
        await ValidateAsync(dto, ct);
        var entity = await uow.Students.GetByIdAsync(dto.Id, ct)
            ?? throw new BusinessRuleException("Учащийся не найден.");
        entity.FullName = dto.FullName.Trim();
        entity.BirthDate = dto.BirthDate;
        entity.CurrentClassId = dto.CurrentClassId;
        entity.ContactInfo = string.IsNullOrWhiteSpace(dto.ContactInfo) ? null : dto.ContactInfo.Trim();
        entity.EnrollmentDate = dto.EnrollmentDate;
        uow.Students.Update(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Students.GetByIdAsync(id, ct)
            ?? throw new BusinessRuleException("Учащийся не найден.");

        if (await uow.Journals.ExistsAsync(j => j.StudentId == id, ct))
            throw new BusinessRuleException("Удаление невозможно: у учащегося есть отметки или пропуски в журнале.");

        uow.Students.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(StudentEditDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new BusinessRuleException("Укажите ФИО учащегося.");
        if (dto.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            throw new BusinessRuleException("Дата рождения не может быть в будущем.");
        if (dto.EnrollmentDate < dto.BirthDate.AddYears(MinEnrollmentAge))
            throw new BusinessRuleException(
                $"На момент зачисления учащемуся должно быть не менее {MinEnrollmentAge} лет.");

        var classId = dto.CurrentClassId;
        if (!await uow.Classes.ExistsAsync(c => c.ClassId == classId, ct))
            throw new BusinessRuleException("Класс не найден.");
    }
}
