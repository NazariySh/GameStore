using FluentValidation;
using FluentValidation.Results;
using Gamestore.BLL.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services;

public class ValidationServiceTests
{
    private readonly Mock<IValidator<TestClass>> _mockValidator;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _validationService = new ValidationService(
            _mockServiceProvider.Object,
            Mock.Of<ILogger<ValidationService>>());
        _mockValidator = new Mock<IValidator<TestClass>>();
    }

    [Fact]
    public async Task ValidateAsync_ShouldThrowArgumentNullException_WhenInstanceIsNull()
    {
        TestClass? instance = null;

        var act = () => _validationService.ValidateAndThrowAsync(instance!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Theory]
    [MemberData(nameof(InvalidTestCases))]
    public async Task ValidateAsync_ShouldThrowValidationException_WhenInstanceIsInvalid(
        TestClass invalidInstance,
        ValidationFailure[] failures)
    {
        SetupMockServiceProviderGetValidator(_mockValidator.Object);
        SetupMockValidatorValidateToFail(failures);

        var act = () => _validationService.ValidateAndThrowAsync(invalidInstance);

        var exception = await Assert.ThrowsAsync<ValidationException>(act);

        Assert.NotNull(exception);
        Assert.Equal(failures.Length, exception.Errors.Count());
        Assert.Equivalent(failures, exception.Errors, strict: true);
    }

    [Fact]
    public async Task ValidateAsync_ShouldNotThrow_WhenValidatorIsNotRegistered()
    {
        var instance = GetInstance();
        IValidator<TestClass>? validator = null;

        SetupMockServiceProviderGetValidator(validator);

        await _validationService.ValidateAndThrowAsync(instance);

        VerifyValidatorCalled(instance, Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_ShouldNotThrow_WhenInstanceIsValid()
    {
        var instance = GetInstance();

        SetupMockServiceProviderGetValidator(_mockValidator.Object);
        SetupMockValidatorValidateToSucceed();

        await _validationService.ValidateAndThrowAsync(instance);

        VerifyValidatorCalled(instance, Times.Once);
    }

    public static TheoryData<TestClass, ValidationFailure[]> InvalidTestCases()
    {
        return new TheoryData<TestClass, ValidationFailure[]>
        {
            {
                new TestClass(),
                new ValidationFailure[] { new("Name", "Required field") }
            },
            {
                new TestClass { Name = null! },
                new ValidationFailure[] { new("Name", "Required field") }
            },
            {
                new TestClass { Name = string.Empty },
                new ValidationFailure[] { new("Name", "Required field") }
            },
            {
                new TestClass { Name = " " },
                new ValidationFailure[] { new("Name", "Required field") }
            },
            {
                new TestClass { Name = "InvalidName##" },
                new ValidationFailure[] { new("Name", "Invalid format") }
            },
        };
    }

    private void SetupMockValidatorValidateToSucceed()
    {
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<TestClass>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupMockValidatorValidateToFail(params ValidationFailure[] failures)
    {
        _mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<TestClass>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));
    }

    private void VerifyValidatorCalled(TestClass instance, Func<Times> times)
    {
        _mockValidator.Verify(
            x => x.ValidateAsync(
                instance,
                It.IsAny<CancellationToken>()),
            times);
    }

    private void SetupMockServiceProviderGetValidator<T>(IValidator<T>? service)
        where T : class
    {
        _mockServiceProvider
            .Setup(provider => provider.GetService(typeof(IValidator<T>)))
            .Returns(service);
    }

    private static TestClass GetInstance()
    {
        return new TestClass { Name = "Valid Instance" };
    }

    public class TestClass
    {
        public string Name { get; set; }
    }
}