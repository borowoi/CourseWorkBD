using SchoolJournal.DataAccess.Entities;

namespace SchoolJournal.DataAccess.Interfaces;

public interface IJournalRepository : IRepository<Journal>
{
    /// <summary>Запрос записей журнала с загруженными учеником, уроком, классом и предметом.</summary>
    IQueryable<Journal> QueryWithReferences();

    /// <summary>Проверяет, есть ли уже запись журнала для ученика на этом уроке.</summary>
    Task<bool> EntryExistsAsync(int studentId, int lessonId, int? exceptId = null,
        CancellationToken cancellationToken = default);
}

public interface ILessonRepository : IRepository<Lesson>
{
    /// <summary>Запрос уроков с загруженными классом, предметом и учителем.</summary>
    IQueryable<Lesson> QueryWithReferences();

    /// <summary>Урок вместе с записями журнала и учениками.</summary>
    Task<Lesson?> GetWithJournalAsync(int id, CancellationToken cancellationToken = default);
}

public interface IStudentRepository : IRepository<Student>
{
    /// <summary>Запрос учащихся с загруженным классом.</summary>
    IQueryable<Student> QueryWithClass();
}

public interface ISubjectHourRepository : IRepository<SubjectHour>
{
    /// <summary>Нагрузка предмета по параллели (составной ключ). Возвращает отслеживаемую сущность.</summary>
    Task<SubjectHour?> GetAsync(int subjectId, int gradeLevel, CancellationToken cancellationToken = default);
}
