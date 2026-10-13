using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolJournal.DataAccess.Data;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly SchoolJournalDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(SchoolJournalDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Set.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);

    public virtual IQueryable<T> Query() => Set.AsNoTracking();
}
