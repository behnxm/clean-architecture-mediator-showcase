using CleanArchitectureMediatorShowcase.Application.Features.Products.GetProducts;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class GetProductsQueryValidatorTests
{
    private readonly GetProductsQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_HasNoErrors()
    {
        var query = new GetProductsQuery("keyboard", 1, 20, "price", "asc");
        var result = _validator.Validate(query);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPageLessThanOne_HasValidationError(int invalidPage)
    {
        var query = new GetProductsQuery(null, invalidPage, 10);
        var result = _validator.Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProductsQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WhenPageSizeOutOfRange_HasValidationError(int invalidPageSize)
    {
        var query = new GetProductsQuery(null, 1, invalidPageSize);
        var result = _validator.Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProductsQuery.PageSize));
    }

    [Fact]
    public void Validate_WhenSortByIsNotInWhitelist_HasValidationError()
    {
        var query = new GetProductsQuery(null, 1, 10, "sql_injection_attempt", "asc");
        var result = _validator.Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProductsQuery.SortBy));
    }

    [Fact]
    public void Validate_WhenSortDirectionIsInvalid_HasValidationError()
    {
        var query = new GetProductsQuery(null, 1, 10, "price", "sideways");
        var result = _validator.Validate(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetProductsQuery.SortDirection));
    }
}
