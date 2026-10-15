using Microsoft.EntityFrameworkCore;
using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

public sealed class SubjectService(IUnitOfWork uow) : ISubjectService
{
    public Task<PagedResult<SubjectDto>> GetPagedAsync(SubjectFilterDto filter, CancellationToken ct = default)
    {
        var q = uow.Subjects.Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            q = q.Where(s => s.SubjectName.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(filter.ExamType))
        {
            var examType = filter.ExamType;
            q = q.Where(s => s.ExamType == examType);
        }

        q = filter.SortOrder switch
        {
            "name_desc" => q.OrderByDescending(s => s.SubjectName),
            "exam" => q.OrderBy(s => s.ExamType).ThenBy(s => s.SubjectName),
            "exam_desc" => q.OrderByDescending(s => s.ExamType).ThenBy(s => s.SubjectName),
            _ => q.OrderBy(s => s.SubjectName)
        };

        return q.ToPagedAsync(filter, s => new SubjectDto
        {
            Id = s.SubjectId,
            SubjectName = s.SubjectName,
            ExamType = s.ExamType,
            LessonsCount = s.Lessons.Count(),
            GradesWithHours = s.SubjectHours.Count()
        }, ct);
    }

    public async Task<SubjectEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var s = await uow.Subjects.GetByIdAsync(id, ct);
        return s is null
            ? null
            : new SubjectEditDto { Id = s.SubjectId, SubjectName = s.SubjectName, ExamType = s.ExamType };
    }

    public async Task<int> CreateAsync(SubjectEditDto dto, CancellationToken ct = default)
    {
        await ValidateAsync(dto, ct);
        var entity = new Subject { SubjectName = dto.SubjectName.Trim(), ExamType = dto.ExamType };
        await uow.Subjects.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.SubjectId;
    }

    public async Task UpdateAsync(SubjectEditDto dto, CancellationToken ct = default)
    {
        await ValidateAsync(dto, ct);
        var entity = await uow.Subjects.GetByIdAsync(dto.Id, ct)
            ?? throw new BusinessRuleException("Предмет не найден.");
        entity.SubjectName = dto.SubjectName.Trim();
        entity.ExamType = dto.ExamType;
        uow.Subjects.Update(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Subjects.GetByIdAsync(id, ct)
            ?? throw new BusinessRuleException("Предмет не найден.");

        if (await uow.Lessons.ExistsAsync(l => l.SubjectId == id, ct))
            throw new BusinessRuleException("Удаление невозможно: по предмету проведены уроки.");

        // Нагрузка по часам удаляется каскадно на стороне БД
        uow.Subjects.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SubjectHourDto>> GetHoursAsync(int subjectId, CancellationToken ct = default)
        => await uow.SubjectHours.Query()
            .Where(h => h.SubjectId == subjectId)
            .OrderBy(h => h.GradeLevel)
            .Select(h => new SubjectHourDto(h.GradeLevel, h.HoursPerWeek))
            .ToListAsync(ct);

    public async Task SetHoursAsync(int subjectId, int gradeLevel, int hoursPerWeek, CancellationToken ct = default)
    {
        if (gradeLevel is < 1 or > 11)
            throw new BusinessRuleException("Параллель должна быть от 1 до 11.");
        if (hoursPerWeek is < 1 or > 10)
            throw new BusinessRuleException("Количество часов в неделю должно быть от 1 до 10.");
        if (!await uow.Subjects.ExistsAsync(s => s.SubjectId == subjectId, ct))
            throw new BusinessRuleException("Предмет не найден.");

        var existing = await uow.SubjectHours.GetAsync(subjectId, gradeLevel, ct);
        if (existing is null)
        {
            await uow.SubjectHours.AddAsync(
                new SubjectHour { SubjectId = subjectId, GradeLevel = gradeLevel, HoursPerWeek = hoursPerWeek }, ct);
        }
        else
        {
            existing.HoursPerWeek = hoursPerWeek;
            uow.SubjectHours.Update(existing);
        }
        await uow.SaveChangesAsync(ct);
    }

    public async Task RemoveHoursAsync(int subjectId, int gradeLevel, CancellationToken ct = default)
    {
        var existing = await uow.SubjectHours.GetAsync(subjectId, gradeLevel, ct)
            ?? throw new BusinessRuleException("Запись о нагрузке не найдена.");
        uow.SubjectHours.Remove(existing);
        await uow.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(SubjectEditDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.SubjectName))
            throw new BusinessRuleException("Укажите наименование предмета.");
        if (!ExamTypes.All.Contains(dto.ExamType))
            throw new BusinessRuleException("Недопустимая форма итоговой аттестации.");

        var name = dto.SubjectName.Trim();
        var id = dto.Id;
        if (await uow.Subjects.ExistsAsync(s => s.SubjectName == name && s.SubjectId != id, ct))
            throw new BusinessRuleException("Предмет с таким наименованием уже существует.");
    }
}
