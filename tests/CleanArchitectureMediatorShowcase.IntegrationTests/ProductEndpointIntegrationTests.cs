using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using CleanArchitectureMediatorShowcase.Application.Common.Models;
using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.WebAPI.Contracts.Requests;
using CleanArchitectureMediatorShowcase.WebAPI.Endpoints;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.IntegrationTests;

public sealed class ProductEndpointIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductEndpointIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullCrudLifecycle_ExecutesSuccessfully()
    {
        // 1. POST: Create Product
        var createRequest = new CreateProductRequest
        {
            Name = $"Integration Test Keyboard {Guid.NewGuid():N}",
            Description = "RGB Mechanical Keyboard for Integration Testing",
            Price = 129.99m,
            Stock = 50
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/products", createRequest);
        postResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        postResponse.Headers.Location.ShouldNotBeNull();

        var created = await postResponse.Content.ReadFromJsonAsync<ProductEndpoints.ProductCreatedResponse>();
        created.ShouldNotBeNull();
        var productId = created.Id;
        productId.ShouldNotBe(Guid.Empty);
        postResponse.Headers.Location!.ToString().ShouldBe($"/api/v1/products/{productId}");

        // 2. GET by Id: Verify created state
        var getResponse = await _client.GetAsync($"/api/v1/products/{productId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
        product.ShouldNotBeNull();
        product.Id.ShouldBe(productId);
        product.Name.ShouldBe(createRequest.Name);
        product.Price.ShouldBe(129.99m);
        product.Stock.ShouldBe(50);

        // 3. PUT: Update Product
        var updateRequest = new UpdateProductRequest
        {
            Name = $"Updated Keyboard {Guid.NewGuid():N}",
            Description = "Updated Description",
            Price = 139.99m
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/v1/products/{productId}", updateRequest);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 4. PATCH: Adjust Stock
        var patchResponse = await _client.PatchAsJsonAsync($"/api/v1/products/{productId}/stock", new AdjustStockRequest { Delta = 25 });
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify updated state
        var getUpdatedResponse = await _client.GetAsync($"/api/v1/products/{productId}");
        var updatedProduct = await getUpdatedResponse.Content.ReadFromJsonAsync<ProductResponse>();
        updatedProduct.ShouldNotBeNull();
        updatedProduct.Name.ShouldBe(updateRequest.Name);
        updatedProduct.Price.ShouldBe(139.99m);
        updatedProduct.Stock.ShouldBe(75);

        // 5. DELETE: Remove product
        var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{productId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 6. GET by Id: Verify 404
        var getDeletedResponse = await _client.GetAsync($"/api/v1/products/{productId}");
        getDeletedResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WhenValidationFails_Returns400WithErrorsDictionary()
    {
        // Arrange: Invalid request with negative price and empty name
        var invalidRequest = new CreateProductRequest
        {
            Name = string.Empty,
            Description = "Description",
            Price = -50m,
            Stock = -5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", invalidRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        json.ShouldNotBeNull();
        json["status"]?.GetValue<int>().ShouldBe(400);
        json["code"]?.GetValue<string>().ShouldBe("General.Validation");

        // RFC 9457 errors dictionary
        var errors = json["errors"] as JsonObject;
        errors.ShouldNotBeNull();
        errors.ContainsKey("Name").ShouldBeTrue();
        errors.ContainsKey("Price").ShouldBeTrue();
        errors.ContainsKey("Stock").ShouldBeTrue();
    }

    [Fact]
    public async Task GetProductById_WhenNotFound_Returns404ProblemDetails()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        json.ShouldNotBeNull();
        json["status"]?.GetValue<int>().ShouldBe(404);
        json["code"]?.GetValue<string>().ShouldBe("Product.NotFound");
    }

    [Fact]
    public async Task CreateProduct_WhenNameDuplicate_Returns409Conflict()
    {
        // Arrange
        var uniqueName = $"Duplicate Test {Guid.NewGuid():N}";
        var first = new CreateProductRequest
        {
            Name = uniqueName,
            Description = "Desc",
            Price = 10m,
            Stock = 1
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/products", first);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act: attempt duplicate creation
        var duplicateResponse = await _client.PostAsJsonAsync("/api/v1/products", first);

        // Assert
        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var json = await duplicateResponse.Content.ReadFromJsonAsync<JsonObject>();
        json.ShouldNotBeNull();
        json["status"]?.GetValue<int>().ShouldBe(409);
        json["code"]?.GetValue<string>().ShouldBe("Product.DuplicateName");
    }

    [Fact]
    public async Task AdjustStock_WhenStockDropsBelowZero_Returns409Conflict()
    {
        // Arrange: Create product with stock 5
        var createRequest = new CreateProductRequest
        {
            Name = $"Stock Test {Guid.NewGuid():N}",
            Description = "Desc",
            Price = 25m,
            Stock = 5
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/products", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<ProductEndpoints.ProductCreatedResponse>();
        var productId = created!.Id;

        // Act: attempt to deduct 10
        var patchResponse = await _client.PatchAsJsonAsync($"/api/v1/products/{productId}/stock", new AdjustStockRequest { Delta = -10 });

        // Assert
        patchResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var json = await patchResponse.Content.ReadFromJsonAsync<JsonObject>();
        json.ShouldNotBeNull();
        json["status"]?.GetValue<int>().ShouldBe(409);
        json["code"]?.GetValue<string>().ShouldBe("Product.InsufficientStock");
    }

    [Fact]
    public async Task SearchAndPagination_ReturnsExpectedSubsets()
    {
        // Act: Request page 1 with pageSize 2
        var response = await _client.GetAsync("/api/v1/products?page=1&pageSize=2");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pagedList = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        pagedList.ShouldNotBeNull();
        pagedList.Items.Count.ShouldBe(2);
        pagedList.Page.ShouldBe(1);
        pagedList.PageSize.ShouldBe(2);
        pagedList.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task SearchTerm_FiltersResultsByText()
    {
        // Act: Search for seeded "Monitor"
        var response = await _client.GetAsync("/api/v1/products?searchTerm=Monitor");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pagedList = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        pagedList.ShouldNotBeNull();
        pagedList.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
        pagedList.Items.ShouldAllBe(p => p.Name.Contains("Monitor", StringComparison.OrdinalIgnoreCase) || p.Description.Contains("Monitor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task PriceSorting_CorrectlyOrdersNumericallyInSqlite()
    {
        // Act: Sort by price ascending
        var ascResponse = await _client.GetAsync("/api/v1/products?page=1&pageSize=50&sortBy=price&sortDirection=asc");
        ascResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var ascList = await ascResponse.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        ascList.ShouldNotBeNull();
        var ascPrices = ascList.Items.Select(i => i.Price).ToList();
        ascPrices.ShouldBe(ascPrices.OrderBy(p => p).ToList());

        // Act: Sort by price descending
        var descResponse = await _client.GetAsync("/api/v1/products?page=1&pageSize=50&sortBy=price&sortDirection=desc");
        descResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var descList = await descResponse.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        descList.ShouldNotBeNull();
        var descPrices = descList.Items.Select(i => i.Price).ToList();
        descPrices.ShouldBe(descPrices.OrderByDescending(p => p).ToList());
    }

    [Fact]
    public async Task CreateProduct_PublishesDomainEvents_ObservableInNotificationTracker()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Event Observable Keyboard {Guid.NewGuid():N}",
            Description = "Testing post-commit domain event dispatching",
            Price = 199.99m,
            Stock = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ProductEndpoints.ProductCreatedResponse>();

        // Assert: Event was dispatched to INotificationHandler and captured in tracker
        _factory.NotificationTracker.PublishedNotifications
            .ShouldContain(n => n.ProductId == created!.Id && n.Name == request.Name && n.Price == request.Price);
    }
}
