using FluentAssertions;
using Isotyp.Application.Common;
using Xunit;

namespace Isotyp.Application.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result<string>.Success("test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Act
        var result = Result<string>.Failure("error message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("error message");
    }

    [Fact]
    public void NonGenericSuccess_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void NonGenericFailure_ShouldCreateFailedResult()
    {
        // Act
        var result = Result.Failure("error message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error message");
    }

    [Fact]
    public void FailureWithMultipleErrors_ShouldStoreAllErrors()
    {
        // Act
        var result = Result<string>.Failure(new[] { "error1", "error2", "error3" });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain("error1");
        result.Errors.Should().Contain("error2");
        result.Errors.Should().Contain("error3");
    }
}
