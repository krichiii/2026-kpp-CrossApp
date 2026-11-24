using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Core.Dto;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public sealed class ProductApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreatesProduct_And_GetByLocation_ReturnsOk()
    {
        // Arrange
        var request = new SaveProductRequest("INT-TEST-001", "Інтеграційний товар", "шт", 75);

        // Act 1: POST дає 201 Created
        var postResponse = await _client.PostAsJsonAsync("/api/products", request);

        // Assert 1: 201 Created
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var location = postResponse.Headers.Location;
        Assert.NotNull(location);

        var created = await postResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.Equal("INT-TEST-001", created.Sku);
        Assert.Equal("Інтеграційний товар", created.Name);

        // Act 2: GET за отриманим Location
        var getResponse = await _client.GetAsync(location);

        // Assert 2: 200 OK
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Sku, fetched.Sku);
        Assert.Equal(created.Quantity, fetched.Quantity);
    }

    [Fact]
    public async Task Post_WithInvalidData_ReturnsBadRequest()
    {
        var invalid = new SaveProductRequest("", "", "", -1);

        var response = await _client.PostAsJsonAsync("/api/products", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithDuplicateSku_ReturnsConflict()
    {
        var request1 = new SaveProductRequest("DUP-TEST", "Товар 1", "шт", 10);
        var request2 = new SaveProductRequest("DUP-TEST", "Товар 2", "шт", 20);

        var res1 = await _client.PostAsJsonAsync("/api/products", request1);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);

        var res2 = await _client.PostAsJsonAsync("/api/products", request2);
        Assert.Equal(HttpStatusCode.Conflict, res2.StatusCode);
    }
}
