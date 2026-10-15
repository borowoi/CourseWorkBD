using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.BusinessLogic.Services;

namespace SchoolJournal.BusinessLogic;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        // Отчёты
        services.AddScoped<IClassJournalService, ClassJournalService>();
        services.AddScoped<IStudentReportService, StudentReportService>();
        services.AddScoped<IPerformanceService, PerformanceService>();
        services.AddScoped<IAttendanceService, AttendanceService>();

        // Операции над данными (CRUD)
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<ILookupService, LookupService>();
        return services;
    }
}
