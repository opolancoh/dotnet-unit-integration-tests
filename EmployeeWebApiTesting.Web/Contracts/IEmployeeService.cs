using EmployeeWebApiTesting.Web.DTOs;
using EmployeeWebApiTesting.Web.Models;

namespace EmployeeWebApiTesting.Web.Contracts;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAll();
    Task<Employee?> GetById(Guid id);
    Task<Employee> Create(EmployeeCreateUpdateDto item);
    Task Update(Guid id, EmployeeCreateUpdateDto item);
    Task Remove(Guid id);
}