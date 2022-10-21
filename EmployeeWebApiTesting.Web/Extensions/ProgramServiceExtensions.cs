using Microsoft.EntityFrameworkCore;
using EmployeeWebApiTesting.Web.Contracts;
using EmployeeWebApiTesting.Web.Data;
using EmployeeWebApiTesting.Web.Services;

namespace EmployeeWebApiTesting.Web.Extensions;

public static class ProgramServiceExtensions
{
    public static void ConfigureDbContext(this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

    public static void ConfigureEmployeeService(this IServiceCollection services) =>
        services.AddScoped<IEmployeeService, EmployeeService>();
}