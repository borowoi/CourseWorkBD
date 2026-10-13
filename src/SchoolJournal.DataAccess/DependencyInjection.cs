using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.DataAccess.Data;
using SchoolJournal.DataAccess.Interfaces;
using SchoolJournal.DataAccess.Repositories;

namespace SchoolJournal.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SchoolJournalDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
