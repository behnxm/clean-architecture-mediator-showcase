using CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_HasNoErrors()
    {
        var command = new CreateProductCommand("Valid Name", "Valid Description", 29.99m, 10);
        var result = _validator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenNameIsEmpty_HasValidationError(string? invalidName)
    {
        var command = new CreateProductCommand(invalidName!, "Valid Description", 29.99m, 10);
        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Validate_WhenNameExceedsMaxLength_HasValidationError()
    {
        var longName = new string('x', Product.NameMaxLength + 1);
        var command = new CreateProductCommand(longName, "Valid Description", 29.99m, 10);
        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Validate_WhenPriceIsNegative_HasValidationError()
    {
        var command = new CreateProductCommand("Valid Name", "Valid Description", -1m, 10);
        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.Price));
    }

    [Fact]
    public void Validate_WhenStockIsNegative_HasValidationError()
    {
        var command = new CreateProductCommand("Valid Name", "Valid Description", 10m, -1);
        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductCommand.Stock));
    }
}
