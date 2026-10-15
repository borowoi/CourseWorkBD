using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

public sealed class TeacherService(IUnitOfWork uow) : ITeacherService
{
    public Task<PagedResult<TeacherDto>> GetPagedAsync(TeacherFilterDto filter, CancellationToken ct = default)
    {
        var q = uow.Teachers.Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            q = q.Where(t => t.FullName.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = filter.Category;
            q = q.Where(t => t.QualificationCategory == category);
        }

        q = filter.SortOrder switch
        {
            "name_desc" => q.OrderByDescending(t => t.FullName),
            "category" => q.OrderBy(t => t.QualificationCategory).ThenBy(t => t.FullName),
            "category_desc" => q.OrderByDescending(t => t.QualificationCategory).ThenBy(t => t.FullName),
            "workload" => q.OrderBy(t => t.WorkloadHours).ThenBy(t => t.FullName),
            "workload_desc" => q.OrderByDescending(t => t.WorkloadHours).ThenBy(t => t.FullName),
            _ => q.OrderBy(t => t.FullName)
        };

        return q.ToPagedAsync(filter, t => new TeacherDto
        {
            Id = t.TeacherId,
            FullName = t.FullName,
            QualificationCategory = t.QualificationCategory,
            WorkloadHours = t.WorkloadHours,
            ClassesCount = t.Classes.Count(),
            LessonsCount = t.Lessons.Count()
        }, ct);
    }

    public async Task<TeacherEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var t = await uow.Teachers.GetByIdAsync(id, ct);
        return t is null
            ? null
            : new TeacherEditDto
            {
                Id = t.TeacherId,
                FullName = t.FullName,
                QualificationCategory = t.QualificationCategory,
                WorkloadHours = t.WorkloadHours
            };
    }

    public async Task<int> CreateAsync(TeacherEditDto dto, CancellationToken ct = default)
    {
        Validate(dto);
        var entity = new Teacher
        {
            FullName = dto.FullName.Trim(),
            QualificationCategory = dto.QualificationCategory,
            WorkloadHours = dto.WorkloadHours
        };
        await uow.Teachers.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.TeacherId;
    }

    public async Task UpdateAsync(TeacherEditDto dto, CancellationToken ct = default)
    {
        Validate(dto);
        var entity = await uow.Teachers.GetByIdAsync(dto.Id, ct)
            ?? throw new BusinessRuleException("Учитель не найден.");
        entity.FullName = dto.FullName.Trim();
        entity.QualificationCategory = dto.QualificationCategory;
        entity.WorkloadHours = dto.WorkloadHours;
        uow.Teachers.Update(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Teachers.GetByIdAsync(id, ct)
            ?? throw new BusinessRuleException("Учитель не найден.");

        if (await uow.Lessons.ExistsAsync(l => l.TeacherId == id, ct))
            throw new BusinessRuleException("Удаление невозможно: за учителем закреплены уроки.");
        if (await uow.Classes.ExistsAsync(c => c.HomeroomTeacherId == id, ct))
            throw new BusinessRuleException("Удаление невозможно: учитель является классным руководителем.");

        uow.Teachers.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    private static void Validate(TeacherEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new BusinessRuleException("Укажите ФИО учителя.");
        if (!QualificationCategories.All.Contains(dto.QualificationCategory))
            throw new BusinessRuleException("Недопустимая квалификационная категория.");
        if (dto.WorkloadHours is < 0 or > 60)
            throw new BusinessRuleException("Нагрузка должна быть от 0 до 60 часов.");
    }
}
