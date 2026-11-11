using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.Web.Helpers;

namespace SchoolJournal.Web.ViewModels;

/// <summary>Базовая модель страницы-списка: страница данных и применённый фильтр.</summary>
public class IndexViewModel<TDto, TFilter> where TFilter : PagingFilter
{
    public PagedResult<TDto> Result { get; set; } = new();
    public TFilter Filter { get; set; } = default!;
}

public class TeacherIndexViewModel : IndexViewModel<TeacherDto, TeacherFilterDto>
{
    public SelectList Categories { get; set; } = null!;
}

public class SubjectIndexViewModel : IndexViewModel<SubjectDto, SubjectFilterDto>
{
    public SelectList ExamTypes { get; set; } = null!;
}

public class ClassIndexViewModel : IndexViewModel<ClassDto, ClassFilterDto>
{
    public SelectList Grades { get; set; } = null!;
    public SelectList Years { get; set; } = null!;
    public SelectList Teachers { get; set; } = null!;
}

public class StudentIndexViewModel : IndexViewModel<StudentDto, StudentFilterDto>
{
    public SelectList Grades { get; set; } = null!;
    public SelectList Classes { get; set; } = null!;
}

public class SubjectHoursViewModel
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public IReadOnlyList<SubjectHourDto> Hours { get; set; } = [];
}

public sealed record PagerItem(string Label, int? Page, bool Active, bool Disabled);

/// <summary>Данные для частичного представления постраничной навигации.</summary>
public class PagerViewModel
{
    public PagerViewModel(int pageIndex, int totalPages, int totalCount, Dictionary<string, string> routeValues)
    {
        PageIndex = pageIndex;
        TotalPages = totalPages;
        TotalCount = totalCount;
        RouteValues = routeValues;
    }

    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public Dictionary<string, string> RouteValues { get; }

    public Dictionary<string, string> RouteFor(int page) => RouteValues.With("PageIndex", page.ToString());

    /// <summary>Кнопки: «Назад», первая, окно из 5 страниц вокруг текущей, последняя, «Вперёд».</summary>
    public IReadOnlyList<PagerItem> Items()
    {
        var list = new List<PagerItem>
        {
            new("«", PageIndex - 1, false, PageIndex <= 1)
        };

        var pages = new SortedSet<int> { 1, TotalPages };
        for (var i = PageIndex - 2; i <= PageIndex + 2; i++)
        {
            if (i >= 1 && i <= TotalPages)
                pages.Add(i);
        }

        var previous = 0;
        foreach (var page in pages)
        {
            if (page - previous > 1)
                list.Add(new PagerItem("…", null, false, true));
            list.Add(new PagerItem(page.ToString(), page, page == PageIndex, false));
            previous = page;
        }

        list.Add(new PagerItem("»", PageIndex + 1, false, PageIndex >= TotalPages));
        return list;
    }
}
