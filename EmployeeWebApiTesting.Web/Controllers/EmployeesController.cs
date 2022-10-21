using EmployeeWebApiTesting.Web.Contracts;
using EmployeeWebApiTesting.Web.DTOs;
using EmployeeWebApiTesting.Web.Exceptions;
using EmployeeWebApiTesting.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeWebApiTesting.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IEnumerable<Employee>> GetAll()
    {
        return await _service.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetById(Guid id)
    {
        var item = await _service.GetById(id);

        if (item == null)
        {
            return NotFound();
        }

        return item;
    }

    [HttpPost]
    public async Task<ActionResult> Create(EmployeeCreateUpdateDto item)
    {
        var newItem = await _service.Create(item);

        return CreatedAtAction(nameof(GetById), new {id = newItem.Id}, newItem);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, EmployeeCreateUpdateDto item)
    {
        try
        {
            await _service.Update(id, item);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        try
        {
            await _service.Remove(id);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }
}