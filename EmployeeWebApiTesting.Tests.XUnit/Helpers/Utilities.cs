using EmployeeWebApiTesting.Web.Data;
using EmployeeWebApiTesting.Web.DTOs;
using EmployeeWebApiTesting.Web.Models;

namespace EmployeeWebApiTesting.Tests.XUnit.Helpers;

public static class Utilities
{
    public static List<Employee> GetEmployees()
    {
        return new List<Employee>
        {
            new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Sheldon Cooper",
                Age = 22,
                HireDate = new DateTime(2000, 04, 26)
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Marie Curie",
                Age = 17,
                HireDate = new DateTime(2005, 10, 20)
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Mary Shelley",
                Age = 14,
                HireDate = new DateTime(2008, 07, 13)
            }
        };
    }
}