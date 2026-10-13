using SchoolJournal.BusinessLogic.DTOs;

namespace SchoolJournal.BusinessLogic.Interfaces;

public interface IClassJournalService
{
    Task<ClassJournalDto?> GetAsync(int classId, int subjectId, int quarter, CancellationToken ct = default);
}

public interface IStudentReportService
{
    Task<StudentPeriodReportDto?> GetAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public interface IPerformanceService
{
    Task<PerformanceReportDto?> GetForClassAsync(int classId, int quarter, CancellationToken ct = default);
    Task<PerformanceReportDto> GetForParallelAsync(int gradeLevel, string academicYear, int quarter, CancellationToken ct = default);
}

public interface IAttendanceService
{
    Task<AttendanceReportDto> GetAsync(DateOnly from, DateOnly to, int? classId = null, int? subjectId = null, CancellationToken ct = default);
}
