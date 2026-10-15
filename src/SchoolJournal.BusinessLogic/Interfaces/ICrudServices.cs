using SchoolJournal.BusinessLogic.DTOs;

namespace SchoolJournal.BusinessLogic.Interfaces;

public interface ITeacherService
{
    Task<PagedResult<TeacherDto>> GetPagedAsync(TeacherFilterDto filter, CancellationToken ct = default);
    Task<TeacherEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(TeacherEditDto dto, CancellationToken ct = default);
    Task UpdateAsync(TeacherEditDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ISubjectService
{
    Task<PagedResult<SubjectDto>> GetPagedAsync(SubjectFilterDto filter, CancellationToken ct = default);
    Task<SubjectEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(SubjectEditDto dto, CancellationToken ct = default);
    Task UpdateAsync(SubjectEditDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    // Количество часов в неделю по параллелям
    Task<IReadOnlyList<SubjectHourDto>> GetHoursAsync(int subjectId, CancellationToken ct = default);
    Task SetHoursAsync(int subjectId, int gradeLevel, int hoursPerWeek, CancellationToken ct = default);
    Task RemoveHoursAsync(int subjectId, int gradeLevel, CancellationToken ct = default);
}

public interface IClassService
{
    Task<PagedResult<ClassDto>> GetPagedAsync(ClassFilterDto filter, CancellationToken ct = default);
    Task<ClassEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(ClassEditDto dto, CancellationToken ct = default);
    Task UpdateAsync(ClassEditDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IStudentService
{
    Task<PagedResult<StudentDto>> GetPagedAsync(StudentFilterDto filter, CancellationToken ct = default);
    Task<StudentEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(StudentEditDto dto, CancellationToken ct = default);
    Task UpdateAsync(StudentEditDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ILessonService
{
    Task<PagedResult<LessonDto>> GetPagedAsync(LessonFilterDto filter, CancellationToken ct = default);
    Task<LessonEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(LessonEditDto dto, CancellationToken ct = default);
    Task UpdateAsync(LessonEditDto dto, CancellationToken ct = default);

    /// <summary>Удаляет урок. Если по уроку есть записи журнала, нужен явный флаг withEntries.</summary>
    Task DeleteAsync(int id, bool withEntries = false, CancellationToken ct = default);
}

public interface IJournalService
{
    Task<PagedResult<JournalEntryDto>> GetPagedAsync(JournalFilterDto filter, CancellationToken ct = default);
    Task<JournalEntryEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(JournalEntryEditDto dto, CancellationToken ct = default);
    Task UpdateAsync(JournalEntryEditDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Справочники для выпадающих списков (контроллеры не обращаются к репозиториям напрямую).</summary>
public interface ILookupService
{
    Task<IReadOnlyList<LookupDto>> GetClassesAsync(string? academicYear = null, CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetSubjectsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetTeachersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetStudentsAsync(int classId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetAcademicYearsAsync(CancellationToken ct = default);
}
