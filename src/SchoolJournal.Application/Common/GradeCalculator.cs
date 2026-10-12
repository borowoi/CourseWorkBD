namespace SchoolJournal.Application.Common;

public static class GradeCalculator
{
    /// <summary>Итоговая отметка ниже этого значения означает «неуспевающий».</summary>
    public const int PassingGrade = 3;

    /// <summary>Средний балл с точностью до сотых.</summary>
    public static double? Average(IEnumerable<int> grades)
    {
        var list = grades.ToList();
        return list.Count == 0
            ? null
            : Math.Round(list.Average(), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Итоговая отметка: среднее, округлённое до целого (как ROUND во view).</summary>
    public static int? FinalGrade(IEnumerable<int> grades)
    {
        var list = grades.ToList();
        return list.Count == 0
            ? null
            : (int)Math.Round(list.Average(), 0, MidpointRounding.AwayFromZero);
    }

    public static bool IsFailing(int? finalGrade) => finalGrade is < PassingGrade;
}
