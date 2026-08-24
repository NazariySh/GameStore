using FluentValidation.TestHelper;
using Gamestore.BLL.Validators.Images;

namespace Gamestore.BLL.Tests.Validators.Images;

public class Base64ImageValidatorTests
{
    private readonly Base64ImageValidator _validator;

    public Base64ImageValidatorTests()
    {
        _validator = new Base64ImageValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Should_HaveError_When_ImageUrlIsEmpty(string invalidUrl)
    {
        var result = await _validator.TestValidateAsync(invalidUrl);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Image URL cannot be empty.");
    }

    [Theory]
    [MemberData(nameof(InvalidBase64FormatData))]
    public async Task Should_HaveError_When_ImageUrlHasInvalidFormat(string invalidUrl)
    {
        var result = await _validator.TestValidateAsync(invalidUrl);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(Base64ImageValidationRules.FormatPatternPatternMessage);
    }

    [Theory]
    [MemberData(nameof(UnsupportedImageFormatData))]
    public async Task Should_HaveError_When_ImageFormatIsNotAllowed(string urlWithUnsupportedFormat)
    {
        var result = await _validator.TestValidateAsync(urlWithUnsupportedFormat);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage($"Image format is not allowed. Allowed formats: {string.Join(", ", Base64ImageValidationRules.AllowedImageFormats)}");
    }

    [Theory]
    [MemberData(nameof(ValidBase64ImageData))]
    public async Task Should_NotHaveError_When_ImageUrlIsValid(string validUrl)
    {
        var result = await _validator.TestValidateAsync(validUrl);

        result.ShouldNotHaveAnyValidationErrors();
    }

    public static TheoryData<string> InvalidBase64FormatData()
    {
        return new TheoryData<string>
        {
            "not-a-data-url",
            "data:image/png",
            "data:image/png;base64",
            "http://example.com/image.png",
            "data:image/pngbase64,SGVsbG8gV29ybGQ=",
            "data;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            "image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            "data:image/png;charset=utf-8;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            string.Empty,
        };
    }

    public static TheoryData<string> UnsupportedImageFormatData()
    {
        return new TheoryData<string>
        {
            "data:image/bmp;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            "data:image/webp;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8Y2lyY2xlIGN4PSI1MCIgY3k9IjUwIiByPSI0MCIgc3Ryb2tlPSJibGFjayIgc3Ryb2tlLXdpZHRoPSIzIiBmaWxsPSJyZWQiIC8+Cjwvc3ZnPg==",
            "data:image/tiff;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            "data:image/ico;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            "data:application/pdf;base64,JVBERi0xLjQKJcOkw7zDtsO8w6XDqcOgCjIgMCBvYmoKPDwKL1R5cGUgL0NhdGFsb2cKL1BhZ2VzIDMgMCBSCi9WZXJzaW9uIC8xLjQKPj4KZW5kb2JqCjMgMCBvYmoKPDwKL1R5cGUgL1BhZ2VzCi9LaWRzIFs0IDAgUl0KL0NvdW50IDEKL01lZGlhQm94IFswLjAwIDI4MC4zMiA2MTIuMDA0IDc5Mi4wMF0KPj4KZW5kb2JqCjQgMCBvYmoKPDwKL1R5cGUgL1BhZ2UK",
            "data:text/plain;base64,SGVsbG8gV29ybGQ=",
        };
    }

    public static TheoryData<string> ValidBase64ImageData()
    {
        var base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

        return new TheoryData<string>
        {
            $"data:image/png;base64,{base64Image}",
            $"data:image/jpg;base64,{base64Image}",
            $"data:image/jpeg;base64,{base64Image}",
            $"data:image/gif;base64,{base64Image}",
        };
    }
}