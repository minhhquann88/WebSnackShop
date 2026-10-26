using Shouldly;
using SnackShop.Domain.Common;

namespace SnackShop.Domain.Tests.Common;

public sealed class ResultTests
{
    private static readonly Error SampleError = Error.NotFound("Test.NotFound", "Not found.");

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var result = Result.Failure(SampleError);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SampleError);
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void GenericFailure_ValueAccess_Throws()
    {
        Result<int> result = SampleError;

        result.IsFailure.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Match_CallsCorrectBranch()
    {
        Result<string> success = "ok";
        Result<string> failure = SampleError;

        success.Match(v => v, e => e.Code).ShouldBe("ok");
        failure.Match(v => v, e => e.Code).ShouldBe("Test.NotFound");
    }

    [Fact]
    public void Failure_WithNoneError_Throws()
    {
        Should.Throw<ArgumentException>(() => Result.Failure(Error.None));
    }
}
