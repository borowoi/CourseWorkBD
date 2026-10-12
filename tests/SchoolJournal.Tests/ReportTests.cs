using SchoolJournal.Application.Common;
using SchoolJournal.Application.Dtos;
using SchoolJournal.Application.Services;
using Xunit;

namespace SchoolJournal.Tests;

public class GradeCalculatorTests
{
    [Fact]
    public void Average_EmptyList_ReturnsNull() =>
        Assert.Null(GradeCalculator.Average(Array.Empty<int>()));

    [Fact]
    public void Average_RoundsToHundredths() =>
        Assert.Equal(4.33, GradeCalculator.Average(new[] { 4, 4, 5 }));

    [Theory]
    [InlineData(new[] { 2, 3 }, 3)]
    [InlineData(new[] { 2, 2, 3 }, 2)]
    [InlineData(new[] { 5, 5, 4 }, 5)]
    public void FinalGrade_RoundsHalfAwayFromZero(int[] grades, int expected) =>
        Assert.Equal(expected, GradeCalculator.FinalGrade(grades));

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, false)]
    public void IsFailing_BelowThree(int grade, bool expected) =>
        Assert.Equal(expected, GradeCalculator.IsFailing(grade));
}

public class AcademicCalendarTests
{
    [Fact]
    public void FirstQuarter_IsSeptemberToOctober()
    {
        var (from, to) = AcademicCalendar.GetQuarterRange("2026/2027", 1);
        Assert.Equal(new DateOnly(2026, 9, 1), from);
        Assert.Equal(new DateOnly(2026, 10, 31), to);
    }

    [Fact]
    public void ThirdQuarter_UsesSecondYear()
    {
        var (from, to) = AcademicCalendar.GetQuarterRange("2026/2027", 3);
        Assert.Equal(new DateOnly(2027, 1, 1), from);
        Assert.Equal(new DateOnly(2027, 2, 28), to);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void InvalidQuarter_Throws(int q) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => AcademicCalendar.GetQuarterRange("2026/2027", q));
}

public class PerformanceServiceTests
{
    private static JournalEntryRecord E(int student, int subject, int grade) =>
        new(1, student, $"S{student}", 1, 5, "А", subject, $"Sub{subject}",
            1, new DateOnly(2026, 9, 10), 1, true, null, grade, null);

    [Fact]
    public void Build_CountsFailingStudentsAndRanksSubjects()
    {
        var entries = new List<JournalEntryRecord>
        {
            E(1, 1, 2), E(1, 1, 2),
            E(2, 1, 5),
            E(1, 2, 5), E(2, 2, 5)
        };

        var report = PerformanceService.Build("5А", "2026/2027", 1, entries);

        Assert.Equal(1, report.FailingStudents);
        Assert.Equal("Sub2", report.Subjects[0].SubjectName);
        Assert.Equal(1, report.Subjects[0].Rank);
        Assert.Equal(1, report.Subjects.Single(s => s.SubjectName == "Sub1").FailingCount);
    }
}

public class AttendanceServiceTests
{
    [Fact]
    public void Build_GroupsAbsencesByReason()
    {
        JournalEntryRecord A(string reason) =>
            new(1, 1, "S", 1, 5, "А", 1, "Алгебра", 1, new DateOnly(2026, 9, 10), 1, false, reason, null, null);

        var report = AttendanceService.Build(
            new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30),
            new[] { A("Болезнь"), A("Болезнь"), A("Уважительная"), A("Без уважительной причины") });

        var row = Assert.Single(report.Rows);
        Assert.Equal(2, row.Illness);
        Assert.Equal(1, row.Valid);
        Assert.Equal(1, row.Unexcused);
        Assert.Equal(4, report.TotalAbsences);
    }
}
