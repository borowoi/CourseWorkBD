using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolJournal.BusinessLogic.DTOs;

namespace SchoolJournal.BusinessLogic.Common;

internal static class PagingExtensions
{
    /// <summary>
    /// Постраничная выборка: общее количество и нужная страница считаются на стороне БД,
    /// проекция в DTO выполняется тем же запросом. Запрос должен быть уже отсортирован.
    /// </summary>
    public static async Task<PagedResult<TDto>> ToPagedAsync<TEntity, TDto>(
        this IQueryable<TEntity> query, PagingFilter paging,
        Expression<Func<TEntity, TDto>> selector, CancellationToken ct)
    {
        var pageSize = Math.Clamp(paging.PageSize, 1, 200);
        var total = await query.CountAsync(ct);
        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var pageIndex = Math.Clamp(paging.PageIndex, 1, lastPage);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(ct);

        return new PagedResult<TDto>
        {
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}
