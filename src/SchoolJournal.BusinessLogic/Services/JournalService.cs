using Microsoft.EntityFrameworkCore;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.BusinessLogic.Services;

/// <summary>Отметки и пропуски: одна запись журнала на учащегося на уроке.</summary>
public sealed class JournalService(IUnitOfWork uow) : IJournalService
{
    public Task<PagedResult<JournalEntryDto>> GetPagedAsync(JournalFilterDto filter, CancellationToken ct = default)
    {
        var q = uow.Journals.Query();

        if (filter.ClassId.HasValue)
        {
            var classId = filter.ClassId.Value;
            q = q.Where(j => j.Lesson.ClassId == classId);
        }
        if (filter.SubjectId.HasValue)
        {
            var subjectId = filter.SubjectId.Value;
            q = q.Where(j => j.Lesson.SubjectId == subjectId);
        }
        if (filter.StudentId.HasValue)
        {
            var studentId = filter.StudentId.Value;
            q = q.Where(j => j.StudentId == studentId);
        }
        if (filter.LessonId.HasValue)
        {
            var lessonId = filter.LessonId.Value;
            q = q.Where(j => j.LessonId == lessonId);
        }
        if (filter.DateFrom.HasValue)
        {
            var from = filter.DateFrom.Value;
            q = q.Where(j => j.Lesson.LessonDate >= from);
        }
        if (filter.DateTo.HasValue)
        {
            var to = filter.DateTo.Value;
            q = q.Where(j => j.Lesson.LessonDate <= to);
        }
        if (filter.OnlyGraded) q = q.Where(j => j.GradeValue != null);
        if (filter.OnlyAbsences) q = q.Where(j => !j.IsPresent);

        q = filter.SortOrder switch
        {
            "date" => q.OrderBy(j => j.Lesson.LessonDate).ThenBy(j => j.Lesson.LessonNumber).ThenBy(j => j.Student.FullName),
            "student" => q.OrderBy(j => j.Student.FullName).ThenByDescending(j => j.Lesson.LessonDate),
            "student_desc" => q.OrderByDescending(j => j.Student.FullName).ThenByDescending(j => j.Lesson.LessonDate),
            "grade" => q.OrderBy(j => j.GradeValue).ThenByDescending(j => j.Lesson.LessonDate),
            "grade_desc" => q.OrderByDescending(j => j.GradeValue).ThenByDescending(j => j.Lesson.LessonDate),
            _ => q.OrderByDescending(j => j.Lesson.LessonDate).ThenBy(j => j.Lesson.LessonNumber).ThenBy(j => j.Student.FullName)
        };

        return q.ToPagedAsync(filter, j => new JournalEntryDto
        {
            Id = j.JournalId,
            StudentId = j.StudentId,
            StudentName = j.Student.FullName,
            GradeLevel = j.Lesson.Class.GradeLevel,
            ClassLetter = j.Lesson.Class.ClassLetter,
            SubjectName = j.Lesson.Subject.SubjectName,
            LessonId = j.LessonId,
            LessonDate = j.Lesson.LessonDate,
            LessonNumber = j.Lesson.LessonNumber,
            IsPresent = j.IsPresent,
            AbsenceReason = j.AbsenceReason,
            GradeValue = j.GradeValue,
            WorkType = j.WorkType
        }, ct);
    }

    public async Task<JournalEntryEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var j = await uow.Journals.GetByIdAsync(id, ct);
        return j is null
            ? null
            : new JournalEntryEditDto
            {
                Id = j.JournalId,
                StudentId = j.StudentId,
                LessonId = j.LessonId,
                IsPresent = j.IsPresent,
                AbsenceReason = j.AbsenceReason,
                GradeValue = j.GradeValue,
                WorkType = j.WorkType
            };
    }

    public async Task<int> CreateAsync(JournalEntryEditDto dto, CancellationToken ct = default)
    {
        var lessonId = dto.LessonId;
        var studentId = dto.StudentId;

        var lesson = await uow.Lessons.Query()
            .Where(l => l.LessonId == lessonId)
            .Select(l => new { l.ClassId })
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException("Урок не найден.");

        var student = await uow.Students.Query()
            .Where(s => s.StudentId == studentId)
            .Select(s => new { s.CurrentClassId })
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException("Учащийся не найден.");

        if (student.CurrentClassId != lesson.ClassId)
            throw new BusinessRuleException("Учащийся не относится к классу, у которого проходит этот урок.");

        if (await uow.Journals.EntryExistsAsync(studentId, lessonId, null, ct))
            throw new BusinessRuleException("Для этого учащегося на данном уроке запись журнала уже существует.");

        var (reason, grade, workType) = Normalize(dto);

        var entity = new Journal
        {
            StudentId = studentId,
            LessonId = lessonId,
            IsPresent = dto.IsPresent,
            AbsenceReason = reason,
            GradeValue = grade,
            WorkType = workType
        };
        await uow.Journals.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.JournalId;
    }

    public async Task UpdateAsync(JournalEntryEditDto dto, CancellationToken ct = default)
    {
        var entity = await uow.Journals.GetByIdAsync(dto.Id, ct)
            ?? throw new BusinessRuleException("Запись журнала не найдена.");

        // Ученик и урок в существующей записи не меняются
        var (reason, grade, workType) = Normalize(dto);
        entity.IsPresent = dto.IsPresent;
        entity.AbsenceReason = reason;
        entity.GradeValue = grade;
        entity.WorkType = workType;

        uow.Journals.Update(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await uow.Journals.GetByIdAsync(id, ct)
            ?? throw new BusinessRuleException("Запись журнала не найдена.");
        uow.Journals.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Проверяет согласованность полей записи (те же правила, что и CHECK-ограничение в БД)
    /// и возвращает нормализованные значения причины, отметки и вида работы.
    /// </summary>
    internal static (string? Reason, int? Grade, string? WorkType) Normalize(JournalEntryEditDto dto)
    {
        var reason = string.IsNullOrWhiteSpace(dto.AbsenceReason) ? null : dto.AbsenceReason.Trim();
        var workType = string.IsNullOrWhiteSpace(dto.WorkType) ? null : dto.WorkType.Trim();

        if (!dto.IsPresent)
        {
            if (reason is null)
                throw new BusinessRuleException("Для пропуска необходимо указать причину.");
            if (!AbsenceReasons.All.Contains(reason))
                throw new BusinessRuleException("Недопустимая причина пропуска.");
            if (dto.GradeValue.HasValue || workType is not null)
                throw new BusinessRuleException("Учащемуся, пропустившему урок, нельзя поставить отметку.");
            return (reason, null, null);
        }

        if (dto.GradeValue is < 2 or > 5)
            throw new BusinessRuleException("Отметка должна быть от 2 до 5.");
        if (dto.GradeValue.HasValue && workType is null)
            throw new BusinessRuleException("Для отметки укажите вид работы.");
        if (!dto.GradeValue.HasValue && workType is not null)
            throw new BusinessRuleException("Вид работы указывается только вместе с отметкой.");
        if (workType is not null && !WorkTypes.All.Contains(workType))
            throw new BusinessRuleException("Недопустимый вид работы.");

        return (null, dto.GradeValue, workType);
    }
}
