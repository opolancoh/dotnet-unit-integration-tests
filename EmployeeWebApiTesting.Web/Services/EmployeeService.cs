using EmployeeWebApiTesting.Web.Contracts;
using EmployeeWebApiTesting.Web.Data;
using EmployeeWebApiTesting.Web.DTOs;
using EmployeeWebApiTesting.Web.Exceptions;
using EmployeeWebApiTesting.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeWebApiTesting.Web.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _db;

    public EmployeeService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Employee>> GetAll()
    {
        return await _db.Employees.ToListAsync();
    }

    public async Task<Employee?> GetById(Guid id)
    {
        return await _db.Employees.SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Employee> Create(EmployeeCreateUpdateDto item)
    {
        var newItem = new Employee
        {
            Id = new Guid(),
            Name = item.Name,
            Age = item.Age.GetValueOrDefault(),
            HireDate = item.HireDate.GetValueOrDefault()
        };

        _db.Employees.Add(newItem);
        await _db.SaveChangesAsync();

        return newItem;
    }

    public async Task Update(Guid id, EmployeeCreateUpdateDto item)
    {
        var itemToUpdate = new Employee
        {
            Id = id,
            Name = item.Name,
            Age = item.Age.GetValueOrDefault(),
            HireDate = item.HireDate.GetValueOrDefault()
        };

        _db.Entry(itemToUpdate).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ItemExists(id))
            {
                throw new EntityNotFoundException(id);
            }
            else
            {
                throw;
            }
        }
    }

    public async Task Remove(Guid id)
    {
        var itemExists = ItemExists(id);

        if (!itemExists)
        {
            throw new EntityNotFoundException(id);
        }

        _db.Employees.Remove(new Employee {Id = id});
        await _db.SaveChangesAsync();
    }

    private bool ItemExists(Guid id)
    {
        return _db.Employees.Any(e => e.Id == id);
    }
}