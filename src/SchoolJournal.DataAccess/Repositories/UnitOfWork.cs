using Microsoft.EntityFrameworkCore.Storage;
using SchoolJournal.DataAccess.Data;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SchoolJournalDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(SchoolJournalDbContext context)
    {
        _context = context;
        Journals = new JournalRepository(context);
        Lessons = new LessonRepository(context);
        Students = new StudentRepository(context);
        SubjectHours = new SubjectHourRepository(context);
        Classes = new Repository<Class>(context);
        Subjects = new Repository<Subject>(context);
        Teachers = new Repository<Teacher>(context);
        FinalGrades = new Repository<VFinalGrade>(context);
    }

    public IJournalRepository Journals { get; }
    public ILessonRepository Lessons { get; }
    public IStudentRepository Students { get; }
    public ISubjectHourRepository SubjectHours { get; }
    public IRepository<Class> Classes { get; }
    public IRepository<Subject> Subjects { get; }
    public IRepository<Teacher> Teachers { get; }
    public IRepository<VFinalGrade> FinalGrades { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
