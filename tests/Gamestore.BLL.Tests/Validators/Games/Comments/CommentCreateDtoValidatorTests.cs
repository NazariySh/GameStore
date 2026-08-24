using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Games.Comments;

namespace Gamestore.BLL.Tests.Validators.Games.Comments;

public class CommentCreateDtoValidatorTests
{
    private readonly CommentCreateDtoValidator _validator;

    public CommentCreateDtoValidatorTests()
    {
        _validator = new CommentCreateDtoValidator();
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_BodyIsEmpty(string invalidBody)
    {
        var request = new CommentCreateDto
        {
            Body = invalidBody,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorMessage("Body is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_BodyIsTooLong()
    {
        var request = new CommentCreateDto
        {
            Body = new string('a', CommentValidationRules.MaxBodyLength + 1),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Body)
            .WithErrorMessage($"Body must not exceed {CommentValidationRules.MaxBodyLength} characters.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_BodyIsValid()
    {
        var request = new CommentCreateDto
        {
            Body = "This is a valid comment body.",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Body);
    }
}