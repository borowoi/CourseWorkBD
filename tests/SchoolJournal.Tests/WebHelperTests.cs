using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.Web.Helpers;
using SchoolJournal.Web.ViewModels;
using Xunit;

namespace SchoolJournal.Tests;

public class RouteHelperTests
{
    [Fact]
    public void ToRouteValues_SkipsEmptyValuesAndExcludedNames()
    {
        var filter = new LessonFilterDto
        {
            ClassId = 5,
            SubjectId = null,
            DateFrom = new DateOnly(2026, 9, 1),
            PageIndex = 3,
            SortOrder = ""
        };

        var values = filter.ToRouteValues("PageIndex");

        Assert.Equal("5", values["ClassId"]);
        Assert.Equal("2026-09-01", values["DateFrom"]);
        Assert.False(values.ContainsKey("SubjectId"));
        Assert.False(values.ContainsKey("PageIndex"));
        Assert.False(values.ContainsKey("SortOrder"));
    }

    [Fact]
    public void ToRouteValues_BooleanFalseIsOmitted()
    {
        var filter = new JournalFilterDto { OnlyAbsences = true, OnlyGraded = false };

        var values = filter.ToRouteValues();

        Assert.Equal("true", values["OnlyAbsences"]);
        Assert.False(values.ContainsKey("OnlyGraded"));
    }

    [Fact]
    public void With_ReturnsCopyAndRemovesKeyOnEmptyValue()
    {
        var source = new Dictionary<string, string> { ["A"] = "1" };

        var changed = source.With("A", "2").With("B", "x");
        var removed = source.With("A", null);

        Assert.Equal("1", source["A"]);
        Assert.Equal("2", changed["A"]);
        Assert.Equal("x", changed["B"]);
        Assert.False(removed.ContainsKey("A"));
    }
}

public class PagerViewModelTests
{
    [Fact]
    public void Items_MarksCurrentPageAndAddsEllipsis()
    {
        var pager = new PagerViewModel(5, 10, 200, new Dictionary<string, string>());

        var items = pager.Items();

        Assert.Single(items, i => i.Active);
        Assert.Equal(5, items.Single(i => i.Active).Page);
        Assert.Equal(2, items.Count(i => i.Label == "…"));
        Assert.Equal("«", items.First().Label);
        Assert.Equal("»", items.Last().Label);
    }

    [Fact]
    public void Items_FirstPage_DisablesPreviousButton()
    {
        var pager = new PagerViewModel(1, 3, 50, new Dictionary<string, string>());

        var items = pager.Items();

        Assert.True(items.First().Disabled);
        Assert.False(items.Last().Disabled);
    }

    [Fact]
    public void RouteFor_ReplacesPageIndexAndKeepsFilters()
    {
        var pager = new PagerViewModel(1, 3, 50, new Dictionary<string, string> { ["Search"] = "Иван" });

        var route = pager.RouteFor(2);

        Assert.Equal("2", route["PageIndex"]);
        Assert.Equal("Иван", route["Search"]);
    }
}
