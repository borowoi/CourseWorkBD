using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.BusinessLogic.Services;

namespace SchoolJournal.BusinessLogic;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<IClassJournalService, ClassJournalService>();
        services.AddScoped<IStudentReportService, StudentReportService>();
        services.AddScoped<IPerformanceService, PerformanceService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        return services;
    }
}
