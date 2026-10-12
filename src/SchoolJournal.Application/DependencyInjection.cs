using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Application.Services;

namespace SchoolJournal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClassJournalService, ClassJournalService>();
        services.AddScoped<IStudentReportService, StudentReportService>();
        services.AddScoped<IPerformanceService, PerformanceService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        return services;
    }
}
