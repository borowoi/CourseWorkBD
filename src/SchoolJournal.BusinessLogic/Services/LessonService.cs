using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

public sealed class LessonService(IUnitOfWork uow) : ILessonService
{
    public Task<PagedResult<LessonDto>> GetPagedAsync(LessonFilterDto filter, CancellationToken ct = default)
    {
        var q = uow.Lessons.Query();

        if (filter.ClassId.HasValue)
        {
            var classId = filter.ClassId.Value;
            q = q.Where(l => l.ClassId == classId);
        }
        if (filter.GradeLevel.HasValue)
        {
            var grade = filter.GradeLevel.Value;
            q = q.Where(l => l.Class.GradeLevel == grade);
        }
        if (filter.SubjectId.HasValue)
        {
            var subjectId = filter.SubjectId.Value;
            q = q.Where(l => l.SubjectId == subjectId);
        }
        if (filter.TeacherId.HasValue)
        {
            var teacherId = filter.TeacherId.Value;
            q = q.Where(l => l.TeacherId == teacherId);
        }
        if (filter.DateFrom.HasValue)
        {
            var from = filter.DateFrom.Value;
            q = q.Where(l => l.LessonDate >= from);
        }
        if (filter.DateTo.HasValue)
        {
            var to = filter.DateTo.Value;
            q = q.Where(l => l.LessonDate <= to);
        }

        q = filter.SortOrder switch
        {
            "date" => q.OrderBy(l => l.LessonDate).ThenBy(l => l.LessonNumber),
            "class" => q.OrderBy(l => l.Class.GradeLevel).ThenBy(l => l.Class.ClassLetter).ThenBy(l => l.LessonDate).ThenBy(l => l.LessonNumber),
            "subject" => q.OrderBy(l => l.Subject.SubjectName).ThenBy(l => l.LessonDate),
            "subject_desc" => q.OrderByDescending(l => l.Subject.SubjectName).ThenBy(l => l.LessonDate),
            _ => q.OrderByDescending(l => l.LessonDate).ThenBy(l => l.LessonNumber)
        };

        return q.ToPagedAsync(filter, l => new LessonDto
        {
            Id = l.LessonId,
            ClassId = l.ClassId,
            GradeLevel = l.Class.GradeLevel,
            ClassLetter = l.Class.ClassLetter,
            SubjectName = l.Subject.SubjectName,
            TeacherName = l.Teacher != null ? l.Teacher.FullName : null,
            LessonDate = l.LessonDate,
            LessonNumber = l.LessonNumber,
            Topic = l.Topic,
            EntriesCount = l.Journals.Count()
        }, ct);
    }

    public async Task<LessonEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var l = await uow.Lessons.GetByIdAsync(id, ct);
        return l is null
            ? null
            : new LessonEditDto
            {
                Id = l.LessonId,
                ClassId = l.ClassId,
                SubjectId = l.SubjectId,
                TeacherId = l.TeacherId,
                LessonDate = l.LessonDate,
                LessonNumber = l.LessonNumber,
                Topic = l.Topic
            };
    }

    public async Task<int> CreateAsync(LessonEditDto dto, CancellationToken ct = default)
    {
        await ValidateAsync(dto, ct);
        var entity = new Lesson
        {
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            LessonDate = dto.LessonDate,
            LessonNumber = dto.LessonNumber,
            Topic = dto.Topic.Trim()
        };
        await uow.Lessons.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.LessonId;
    }

    public async Task UpdateAsync(LessonEditDto dto, CancellationToken ct = default)
    {
        await ValidateAsync(dto, ct);
        var entity = await uow.Lessons.GetByIdAsync(dto.Id, ct)
            ?? throw new BusinessRuleException("Урок не найден.");

        // Перенос урока в другой класс сделал бы записи журнала некорректными
        if (entity.ClassId != dto.ClassId &&
            await uow.Journals.ExistsAsync(j => j.LessonId == dto.Id, ct))
            throw new BusinessRuleException("Нельзя сменить класс урока, по которому уже есть записи журнала.");

        entity.ClassId = dto.ClassId;
        entity.SubjectId = dto.SubjectId;
        entity.TeacherId = dto.TeacherId;
        entity.LessonDate = dto.LessonDate;
        entity.LessonNumber = dto.LessonNumber;
        entity.Topic = dto.Topic.Trim();
        uow.Lessons.Update(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, bool withEntries = false, CancellationToken ct = default)
    {
        // Урок загружается вместе с записями журнала (отслеживаемые сущности)
        var entity = await uow.Lessons.GetWithJournalAsync(id, ct)
            ?? throw new BusinessRuleException("Урок не найден.");

        var entries = entity.Journals.ToList();
        if (entries.Count > 0)
        {
            if (!withEntries)
                throw new BusinessRuleException(
                    $"По уроку есть записи журнала ({entries.Count}). Подтвердите удаление вместе с записями.");
            foreach (var entry in entries)
                uow.Journals.Remove(entry);
        }

        uow.Lessons.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(LessonEditDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Topic))
            throw new BusinessRuleException("Укажите тему урока.");
        if (dto.LessonNumber is < 1 or > 8)
            throw new BusinessRuleException("Номер урока должен быть от 1 до 8.");
        if (dto.LessonDate.DayOfWeek == DayOfWeek.Sunday)
            throw new BusinessRuleException("Урок не может проходить в воскресенье.");

        var classId = dto.ClassId;
        var subjectId = dto.SubjectId;
        var teacherId = dto.TeacherId;
        var date = dto.LessonDate;
        var number = dto.LessonNumber;
        var id = dto.Id;

        if (!await uow.Classes.ExistsAsync(c => c.ClassId == classId, ct))
            throw new BusinessRuleException("Класс не найден.");
        if (!await uow.Subjects.ExistsAsync(s => s.SubjectId == subjectId, ct))
            throw new BusinessRuleException("Предмет не найден.");
        if (teacherId.HasValue && !await uow.Teachers.ExistsAsync(t => t.TeacherId == teacherId, ct))
            throw new BusinessRuleException("Учитель не найден.");

        // Правило расписания: у класса не может быть двух уроков одновременно
        if (await uow.Lessons.ExistsAsync(
                l => l.ClassId == classId && l.LessonDate == date && l.LessonNumber == number && l.LessonId != id, ct))
            throw new BusinessRuleException("В это время у класса уже есть другой урок.");

        // Правило расписания: учитель не может вести два урока одновременно
        if (teacherId.HasValue && await uow.Lessons.ExistsAsync(
                l => l.TeacherId == teacherId && l.LessonDate == date && l.LessonNumber == number && l.LessonId != id, ct))
            throw new BusinessRuleException("В это время учитель уже ведёт другой урок.");
    }
}
