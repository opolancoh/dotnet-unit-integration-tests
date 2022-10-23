using System.Net;
using System.Text;
using System.Text.Json;
using EmployeeWebApiTesting.Tests.XUnit.Helpers;
using EmployeeWebApiTesting.Web.Models;

namespace EmployeeWebApiTesting.Tests.XUnit.IntegrationTests;

public class EmployeeIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly JsonSerializerOptions _options;
    private readonly HttpClient _httpClient;

    public EmployeeIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();

        _options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region GetAll

    [Fact]
    public async Task GeAll_ShouldReturnAllEmployees()
    {
        var response = await _httpClient.GetAsync("/api/employees");
        var payloadString = await response.Content.ReadAsStringAsync();
        var payloadObject = JsonSerializer.Deserialize<List<Employee>>(payloadString, _options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Utilities.GetEmployees().Count, payloadObject?.Count);

        var item = payloadObject?.SingleOrDefault(x => x.Name == "Sheldon Cooper");
        Assert.Equal(22, item?.Age);
        Assert.Equal(new DateTime(2000, 04, 26), item?.HireDate);
    }

    #endregion

    #region GeById

    [Fact]
    public async Task GeById_ShouldReturnOnlyOneEmployee()
    {
        var itemsResponse = await _httpClient.GetAsync($"/api/employees");
        var itemsPayloadString = await itemsResponse.Content.ReadAsStringAsync();
        var itemsPayloadObject = JsonSerializer.Deserialize<List<Employee>>(itemsPayloadString, _options);
        var item = itemsPayloadObject?.SingleOrDefault(x => x.Name == "Sheldon Cooper");

        var response = await _httpClient.GetAsync($"/api/employees/{item?.Id}");
        var payloadString = await response.Content.ReadAsStringAsync();
        var payloadObject = JsonSerializer.Deserialize<Employee>(payloadString, _options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(22, payloadObject?.Age);
        Assert.Equal(new DateTime(2000, 04, 26), payloadObject?.HireDate);
    }

    [Fact]
    public async Task GeById_ShouldReturnNotFoundWhenIdNotExists()
    {
        var id = new Guid();
        var response = await _httpClient.GetAsync($"/api/employees/{id.ToString()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GeById_ShouldReturnBadRequestWhenIdIsNotValid()
    {
        const string id = "not-valid-id";
        var response = await _httpClient.GetAsync($"/api/employees/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_ShouldCreateAnEmployee()
    {
        var newItem = new
        {
            Name = "Brian O'Conner",
            Age = 18,
            HireDate = new DateTime(2022, 10, 22)
        };

        var payload = JsonSerializer.Serialize(newItem, _options);
        HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/api/employees", httpContent);
        var payloadString = await response.Content.ReadAsStringAsync();
        var payloadObject = JsonSerializer.Deserialize<Employee>(payloadString, _options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotEqual(Guid.Empty, payloadObject?.Id);
        Assert.Equal(newItem.Name, payloadObject?.Name);
        Assert.Equal(newItem.Age, payloadObject?.Age);
        Assert.Equal(newItem.HireDate, payloadObject?.HireDate);
    }

    [Theory]
    [MemberData(nameof(MissingRequiredFieldsData))]
    public async Task Create_ShouldReturnBadRequestWhenMissingRequiredFields(string[] expectedCollection, object payloadObject)
    {
        var payload = JsonSerializer.Serialize(payloadObject, _options);
        HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/api/employees", httpContent);
        var payloadString = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.All(expectedCollection, expected => Assert.Contains(expected, payloadString));
    }

    [Theory]
    [MemberData(nameof(FieldsAreInvalidData))]
    public async Task Create_ShouldReturnBadRequestWhenFieldsAreInvalid(string[] expectedCollection, object payloadObject)
    {
        var payload = JsonSerializer.Serialize(payloadObject, _options);
        HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/api/employees", httpContent);
        var payloadString = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.All(expectedCollection, expected => Assert.Contains(expected, payloadString));
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldUpdateAnEmployee()
    {
        // Create a new item
        var newItem = new
        {
            Name = "Employee Update",
            Age = 100,
            HireDate = new DateTime(2001, 01, 01)
        };

        var newItemPayload = JsonSerializer.Serialize(newItem, _options);
        var newItemHttpContent = new StringContent(newItemPayload, Encoding.UTF8, "application/json");
        var newItemResponse = await _httpClient.PostAsync($"/api/employees", newItemHttpContent);
        var newItemPayloadString = await newItemResponse.Content.ReadAsStringAsync();
        var newItemPayloadObject = JsonSerializer.Deserialize<Employee>(newItemPayloadString, _options);

        // Update the created item
        var updatedItem = new
        {
            Name = "Employee Updated",
            Age = 101,
            HireDate = new DateTime(2002, 02, 02)
        };

        var payload = JsonSerializer.Serialize(updatedItem, _options);
        var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/api/employees/{newItemPayloadObject?.Id}", httpContent);

        // Ensure the item has been changed getting the item from the DB
        var updatedItemResponse = await _httpClient.GetAsync($"/api/employees/{newItemPayloadObject?.Id}");
        var updatedItemPayloadString = await updatedItemResponse.Content.ReadAsStringAsync();
        var updatedItemPayloadObject = JsonSerializer.Deserialize<Employee>(updatedItemPayloadString, _options);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(updatedItem.Name, updatedItemPayloadObject?.Name);
        Assert.Equal(updatedItem.Age, updatedItemPayloadObject?.Age);
        Assert.Equal(updatedItem.HireDate, updatedItemPayloadObject?.HireDate);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFoundWhenIdNotExists()
    {
        var id = new Guid();
        var updatedItem = new
        {
            Name = "Employee Second",
            Age = 101,
            HireDate = new DateTime(2002, 02, 02)
        };

        var payload = JsonSerializer.Serialize(updatedItem, _options);
        var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/api/employees/{id.ToString()}", httpContent);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(MissingRequiredFieldsData))]
    public async Task Update_ShouldReturnBadRequestWhenMissingRequiredFields(string[] expectedCollection, object payloadObject)
    {
        var payload = JsonSerializer.Serialize(payloadObject, _options);
        HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var id = new Guid();
        var response = await _httpClient.PutAsync($"/api/employees/{id.ToString()}", httpContent);
        var payloadString = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.All(expectedCollection, expected => Assert.Contains(expected, payloadString));
    }

    [Theory]
    [MemberData(nameof(FieldsAreInvalidData))]
    public async Task Update_ShouldReturnBadRequestWhenFieldsAreInvalid(string[] expectedCollection, object payloadObject)
    {
        var payload = JsonSerializer.Serialize(payloadObject, _options);
        HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

        var id = new Guid();
        var response = await _httpClient.PutAsync($"/api/employees/{id.ToString()}", httpContent);
        var payloadString = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.All(expectedCollection, expected => Assert.Contains(expected, payloadString));
    }

    #endregion

    #region Remove

    [Fact]
    public async Task Remove_ShouldRemoveOnlyOneEmployee()
    {
        // Create a new item
        var newItem = new
        {
            Name = "Employee Remove",
            Age = 100,
            HireDate = new DateTime(2001, 01, 01)
        };

        var newItemPayload = JsonSerializer.Serialize(newItem, _options);
        var newItemHttpContent = new StringContent(newItemPayload, Encoding.UTF8, "application/json");
        var newItemResponse = await _httpClient.PostAsync($"/api/employees", newItemHttpContent);
        var newItemPayloadString = await newItemResponse.Content.ReadAsStringAsync();
        var newItemPayloadObject = JsonSerializer.Deserialize<Employee>(newItemPayloadString, _options);

        // Remove the created item
        var response = await _httpClient.DeleteAsync($"/api/employees/{newItemPayloadObject?.Id}");

        // Ensure the item has been deleted trying to get the item from the DB
        var deletedItemResponse = await _httpClient.GetAsync($"/api/employees/{newItemPayloadObject?.Id}");

        Assert.Equal(HttpStatusCode.Created, newItemResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deletedItemResponse.StatusCode);
    }

    [Fact]
    public async Task Remove_ShouldReturnNotFoundWhenIdNotExists()
    {
        var id = new Guid();
        var response = await _httpClient.DeleteAsync($"/api/employees/{id.ToString()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Remove_ShouldReturnBadRequestWhenIdIsNotValid()
    {
        const string id = "not-valid-id";
        var response = await _httpClient.DeleteAsync($"/api/employees/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    public static TheoryData<string[], object> MissingRequiredFieldsData => new()
    {
        {new[] {"The Name field is required.", "The Age field is required.", "The HireDate field is required."}, new { }},
        {new[] {"The Name field is required."}, new {Age = 22, HireDate = new DateTime(2022, 10, 22)}},
        {new[] {"The Age field is required."}, new {Name = "Employee 01", HireDate = new DateTime(2022, 10, 22)}},
        {new[] {"The HireDate field is required."}, new {Name = "Employee 01", Age = 22}},
        {new[] {"The Name field is required."}, new {Name = "", Age = 22, HireDate = new DateTime(2022, 10, 22)}},
        {new[] {"The Name field is required."}, new {Name = " ", Age = 22, HireDate = new DateTime(2022, 10, 22)}}
    };

    public static TheoryData<string[], object> FieldsAreInvalidData => new()
    {
        {
            new[]
            {
                "The Name field is invalid.",
                "The Age field is invalid.",
                "The HireDate field is invalid."
            },
            new {Name = "N@me", Age = 17, HireDate = default(DateTime)}
        }
    };
}