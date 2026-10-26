using CleanArchitectureMediatorShowcase.Application.Common.Models;
using CleanArchitectureMediatorShowcase.Application.Features.Products.AdjustProductStock;
using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;
using CleanArchitectureMediatorShowcase.Application.Features.Products.DeleteProduct;
using CleanArchitectureMediatorShowcase.Application.Features.Products.GetProductById;
using CleanArchitectureMediatorShowcase.Application.Features.Products.GetProducts;
using CleanArchitectureMediatorShowcase.Application.Features.Products.UpdateProduct;
using CleanArchitectureMediatorShowcase.WebAPI.Contracts.Requests;
using CleanArchitectureMediatorShowcase.WebAPI.Extensions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitectureMediatorShowcase.WebAPI.Endpoints;

/// <summary>
/// Minimal API route mappings for the Product Catalog resource.
/// Demonstrates thin endpoint architecture: zero business logic, typed result unions,
/// and delegation to the compile-time mediator via <see cref="ISender"/>.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Response model returned upon successful product creation.
    /// </summary>
    /// <param name="Id">The unique identifier of the newly created product.</param>
    public sealed record ProductCreatedResponse(Guid Id);

    /// <summary>
    /// Maps all product-related REST endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route group builder for further chaining.</returns>
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products");

        // GET /api/v1/products
        group.MapGet("/", async Task<Results<Ok<PagedList<ProductResponse>>, ProblemHttpResult>> (
            [AsParameters] GetProductsParameters parameters,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductsQuery(
                parameters.SearchTerm,
                parameters.Page,
                parameters.PageSize,
                parameters.SortBy,
                parameters.SortDirection);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : result.ToHttpResult(httpContext);
        })
        .WithName("GetProducts")
        .WithSummary("List and search products")
        .WithDescription("Retrieves a paginated list of catalog products matching optional search and sorting criteria.");

        // GET /api/v1/products/{id}
        group.MapGet("/{id:guid}", async Task<Results<Ok<ProductResponse>, ProblemHttpResult>> (
            Guid id,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductByIdQuery(id);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : result.ToHttpResult(httpContext);
        })
        .WithName("GetProductById")
        .WithSummary("Get a product by ID")
        .WithDescription("Returns detailed product information for the specified unique identifier.");

        // POST /api/v1/products
        group.MapPost("/", async Task<Results<Created<ProductCreatedResponse>, ProblemHttpResult>> (
            CreateProductRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Description,
                request.Price,
                request.Stock);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsSuccess
                ? TypedResults.Created($"/api/v1/products/{result.Value}", new ProductCreatedResponse(result.Value))
                : result.ToHttpResult(httpContext);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithDescription("Creates a new product item in the catalog and returns the created identifier and resource location.");

        // PUT /api/v1/products/{id}
        group.MapPut("/{id:guid}", async Task<Results<Ok, ProblemHttpResult>> (
            Guid id,
            UpdateProductRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductCommand(
                id,
                request.Name,
                request.Description,
                request.Price);

            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsSuccess
                ? TypedResults.Ok()
                : result.ToHttpResult(httpContext);
        })
        .WithName("UpdateProduct")
        .WithSummary("Update a product")
        .WithDescription("Updates the name, description, and price of an existing catalog product.");

        // PATCH /api/v1/products/{id}/stock
        group.MapPatch("/{id:guid}/stock", async Task<Results<Ok, ProblemHttpResult>> (
            Guid id,
            AdjustStockRequest request,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new AdjustProductStockCommand(id, request.Delta);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsSuccess
                ? TypedResults.Ok()
                : result.ToHttpResult(httpContext);
        })
        .WithName("AdjustProductStock")
        .WithSummary("Adjust product stock")
        .WithDescription("Increases or decreases inventory stock for a product.");

        // DELETE /api/v1/products/{id}
        group.MapDelete("/{id:guid}", async Task<Results<NoContent, ProblemHttpResult>> (
            Guid id,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return result.IsSuccess
                ? TypedResults.NoContent()
                : result.ToHttpResult(httpContext);
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .WithDescription("Deletes a product from the catalog.");

        return group;
    }
}
