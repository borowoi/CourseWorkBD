using System.Linq.Expressions;

namespace SchoolJournal.DataAccess.Interfaces;

/// <summary>Обобщённый репозиторий для операций с сущностями произвольного типа.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);

    /// <summary>Возвращает запрос без отслеживания изменений для построения выборок.</summary>
    IQueryable<T> Query();
}
