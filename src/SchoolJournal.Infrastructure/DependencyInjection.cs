using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Application.Abstractions;
using SchoolJournal.Infrastructure.Persistence;

namespace SchoolJournal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SchoolJournal.Infrastructure.Persistence.SchoolJournalDbContext>(
    o => o.UseSqlServer(connectionString));
        services.AddScoped<IJournalReadRepository, JournalReadRepository>();
        return services;
    }
}