namespace SchoolJournal.BusinessLogic.Common;

public static class ExamTypes
{
    public const string Exam = "Экзамен";
    public const string Credit = "Зачет";
    public const string DifferentiatedCredit = "Дифференцированный зачет";

    public static readonly string[] All = [Exam, Credit, DifferentiatedCredit];
}

public static class WorkTypes
{
    public const string Oral = "Устный ответ";
    public const string Independent = "Самостоятельная";
    public const string Test = "Контрольная";
    public const string Laboratory = "Лабораторная";

    public static readonly string[] All = [Oral, Independent, Test, Laboratory];
}

public static class QualificationCategories
{
    public const string Highest = "Высшая";
    public const string First = "Первая";
    public const string Second = "Вторая";
    public const string None = "Без категории";

    public static readonly string[] All = [Highest, First, Second, None];
}
