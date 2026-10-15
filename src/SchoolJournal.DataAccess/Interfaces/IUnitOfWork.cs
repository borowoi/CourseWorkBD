using SchoolJournal.DataAccess.Entities;

namespace SchoolJournal.DataAccess.Interfaces;

/// <summary>Единица работы: объединяет репозитории и управляет сохранением изменений.</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IJournalRepository Journals { get; }
    ILessonRepository Lessons { get; }
    IStudentRepository Students { get; }
    ISubjectHourRepository SubjectHours { get; }
    IRepository<Class> Classes { get; }
    IRepository<Subject> Subjects { get; }
    IRepository<Teacher> Teachers { get; }

    /// <summary>Представление v_FinalGrades (только чтение; GetByIdAsync не поддерживается).</summary>
    IRepository<VFinalGrade> FinalGrades { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
