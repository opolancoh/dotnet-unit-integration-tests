namespace EmployeeWebApiTesting.Web.DTOs;

public record EmployeeCreateUpdateDto
{
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime HireDate { get; set; }
}