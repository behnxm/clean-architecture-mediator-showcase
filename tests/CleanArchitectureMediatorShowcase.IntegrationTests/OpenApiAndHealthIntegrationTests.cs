using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.IntegrationTests;

public sealed class OpenApiAndHealthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OpenApiAndHealthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LivenessHealthEndpoint_Returns200Ok()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldBe("Healthy");
    }

    [Fact]
    public async Task ReadinessHealthEndpoint_Returns200Ok()
    {
        var response = await _client.GetAsync("/health/ready");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldBe("Healthy");
    }

    [Fact]
    public async Task OpenApiDocument_GeneratesWithUniqueOperationIds()
    {
        // Act
        var response = await _client.GetAsync("/openapi/v1.json");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        json.ShouldNotBeNull();

        var paths = json["paths"] as JsonObject;
        paths.ShouldNotBeNull();

        var operationIds = new List<string>();

        foreach (var pathEntry in paths)
        {
            var pathItem = pathEntry.Value as JsonObject;
            if (pathItem is null) continue;

            foreach (var opEntry in pathItem)
            {
                var operation = opEntry.Value as JsonObject;
                if (operation is null) continue;

                var operationId = operation["operationId"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(operationId))
                {
                    operationIds.Add(operationId);
                }
            }
        }

        operationIds.Count.ShouldBeGreaterThanOrEqualTo(6);

        // Verify all operation IDs are unique
        operationIds.Distinct().Count().ShouldBe(operationIds.Count);

        // Verify expected endpoints operation IDs are present
        operationIds.ShouldContain("GetProducts");
        operationIds.ShouldContain("GetProductById");
        operationIds.ShouldContain("CreateProduct");
        operationIds.ShouldContain("UpdateProduct");
        operationIds.ShouldContain("AdjustProductStock");
        operationIds.ShouldContain("DeleteProduct");
    }
}
