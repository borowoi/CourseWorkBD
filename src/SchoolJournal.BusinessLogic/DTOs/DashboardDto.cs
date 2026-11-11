namespace SchoolJournal.BusinessLogic.DTOs;

/// <summary>Сводка для главной страницы.</summary>
public sealed record DashboardDto(
    int Teachers, int Subjects, int Classes, int Students, int Lessons, int JournalEntries);
