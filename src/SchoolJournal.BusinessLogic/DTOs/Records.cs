namespace SchoolJournal.BusinessLogic.DTOs;

// ---------- справочные ----------
public sealed record ClassInfo(int ClassId, int GradeLevel, string ClassLetter, string AcademicYear)
{
    public string Name => $"{GradeLevel}{ClassLetter}";
}
public sealed record StudentInfo(int StudentId, string FullName, int ClassId, string ClassName);
public sealed record SubjectInfo(int SubjectId, string SubjectName);

// ---------- плоская запись журнала ----------
public sealed record JournalEntryRecord(
    int JournalId, int StudentId, string StudentName,
    int ClassId, int GradeLevel, string ClassLetter,
    int SubjectId, string SubjectName,
    int LessonId, DateOnly LessonDate, int LessonNumber,
    bool IsPresent, string? AbsenceReason, int? GradeValue, string? WorkType);

public sealed class JournalEntryFilter
{
    public int? ClassId { get; init; }
    public int? GradeLevel { get; init; }
    public string? AcademicYear { get; init; }
    public int? StudentId { get; init; }
    public int? SubjectId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public bool OnlyGraded { get; init; }
    public bool OnlyAbsences { get; init; }
}

/// <summary>Строка представления v_FinalGrades.</summary>
public sealed record FinalGradeRecord(
    int StudentId, int SubjectId, string SubjectName,
    string AcademicYear, string PeriodType, int? FinalGrade);

// ---------- журнал класса ----------
public sealed record LessonColumn(int LessonId, DateOnly Date, int Number);
public sealed record JournalCell(int? Grade, bool IsPresent, string? AbsenceReason, string? WorkType);
public sealed record ClassJournalRow(
    int StudentId, string FullName,
    IReadOnlyDictionary<int, JournalCell> Cells,
    double? Average, int? FinalGrade, int Absences);
public sealed record ClassJournalDto(
    ClassInfo Class, SubjectInfo Subject, int Quarter, DateOnly From, DateOnly To,
    IReadOnlyList<LessonColumn> Lessons, IReadOnlyList<ClassJournalRow> Rows);

// ---------- отметки и пропуски учащегося ----------
public sealed record StudentMark(DateOnly Date, int LessonNumber, string Subject, int Grade, string? WorkType);
public sealed record StudentAbsence(DateOnly Date, int LessonNumber, string Subject, string Reason);
public sealed record StudentSubjectAverage(int SubjectId, string SubjectName, int MarksCount, double Average, int FinalGrade);
public sealed record StudentPeriodReportDto(
    StudentInfo Student, DateOnly From, DateOnly To,
    IReadOnlyList<StudentMark> Marks,
    IReadOnlyList<StudentAbsence> Absences,
    IReadOnlyList<StudentSubjectAverage> SubjectAverages,
    IReadOnlyList<FinalGradeRecord> FinalGrades,
    double? OverallAverage);

// ---------- успеваемость ----------
public sealed record SubjectPerformance(
    int Rank, int SubjectId, string SubjectName,
    int GradedStudents, double? Average, int FailingCount);
public sealed record ClassPerformance(
    string ClassName, int GradedStudents, double? Average, int FailingStudents);
public sealed record PerformanceReportDto(
    string Title, string AcademicYear, int Quarter,
    double? OverallAverage, int FailingStudents,
    IReadOnlyList<SubjectPerformance> Subjects,
    IReadOnlyList<ClassPerformance> Classes);

// ---------- посещаемость ----------
public sealed record AttendanceRow(
    string ClassName, string SubjectName,
    int Illness, int Valid, int Unexcused, int Total);
public sealed record AttendanceReportDto(
    DateOnly From, DateOnly To, IReadOnlyList<AttendanceRow> Rows, int TotalAbsences);
