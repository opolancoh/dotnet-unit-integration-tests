using EmployeeWebApiTesting.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeWebApiTesting.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
}