using CleanArchitectureMediatorShowcase.Domain.Common;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Domain;

public sealed class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        var error = Error.Validation("Code", "Description");
        var result = Result.Failure(error);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void TypedResult_Success_ReturnsValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void TypedResult_Failure_AccessingValueThrowsInvalidOperationException()
    {
        var error = Error.NotFound("Item.NotFound", "Item not found");
        var result = Result<int>.Failure(error);

        result.IsFailure.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessfulResult()
    {
        Result<string> result = "Hello World";

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("Hello World");
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailedResult()
    {
        var error = Error.Conflict("Resource.Conflict", "Conflict occurred");
        Result result = error;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void ValidationError_StoresCollectionOfErrors()
    {
        var error1 = Error.Validation("Name", "Name is required");
        var error2 = Error.Validation("Price", "Price must be positive");

        var validationError = new ValidationError([error1, error2]);

        validationError.Type.ShouldBe(ErrorType.Validation);
        validationError.Errors.Count.ShouldBe(2);
        validationError.Errors.ShouldContain(error1);
        validationError.Errors.ShouldContain(error2);
    }
}
