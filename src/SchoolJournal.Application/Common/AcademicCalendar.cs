namespace SchoolJournal.Application.Common;

public static class AcademicCalendar
{
    public static (DateOnly From, DateOnly To) GetQuarterRange(string academicYear, int quarter)
    {
        if (quarter is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "Четверть должна быть от 1 до 4.");

        if (academicYear is null || academicYear.Length != 9 ||
            !int.TryParse(academicYear.AsSpan(0, 4), out var start))
            throw new ArgumentException("Учебный год должен быть в формате 2025/2026.", nameof(academicYear));

        var end = start + 1;
        return quarter switch
        {
            1 => (new DateOnly(start, 9, 1), new DateOnly(start, 10, 31)),
            2 => (new DateOnly(start, 11, 1), new DateOnly(start, 12, 31)),
            3 => (new DateOnly(end, 1, 1), new DateOnly(end, 2, DateTime.DaysInMonth(end, 2))),
            _ => (new DateOnly(end, 3, 1), new DateOnly(end, 5, 31))
        };
    }
}
