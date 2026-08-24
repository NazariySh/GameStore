using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Orders;
using Gamestore.BLL.Validators.Orders;

namespace Gamestore.BLL.Tests.Validators.Orders;

public class UpdateQuantityRequestValidatorTests
{
    private readonly UpdateQuantityRequestValidator _validator;

    public UpdateQuantityRequestValidatorTests()
    {
        _validator = new UpdateQuantityRequestValidator();
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    public async Task Should_HaveError_When_CountIsNegativeOrZero(int count)
    {
        var request = new UpdateQuantityRequest
        {
            Count = count,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Count)
            .WithErrorMessage("Count cannot be negative or zero.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_RequestIsValid()
    {
        var request = new UpdateQuantityRequest
        {
            Count = 5,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}