using SchoolJournal.Application.Dtos;

namespace SchoolJournal.Application.Abstractions;

public interface IJournalReadRepository
{
    Task<ClassInfo?> GetClassAsync(int classId, CancellationToken ct = default);
    Task<StudentInfo?> GetStudentAsync(int studentId, CancellationToken ct = default);
    Task<SubjectInfo?> GetSubjectAsync(int subjectId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentInfo>> GetStudentsByClassAsync(int classId, CancellationToken ct = default);

    Task<IReadOnlyList<JournalEntryRecord>> GetEntriesAsync(JournalEntryFilter filter, CancellationToken ct = default);

    /// <summary>Данные представления v_FinalGrades по учащемуся.</summary>
    Task<IReadOnlyList<FinalGradeRecord>> GetFinalGradesAsync(int studentId, CancellationToken ct = default);
}
