using FluentValidation;
using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Validators.Games.Comments;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Moq;

namespace Gamestore.BLL.Tests.Validators.Games.Comments;

public class CreateCommentRequestValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Comment> _commentRepository;
    private readonly Mock<IValidator<CommentCreateDto>> _mockCommentValidator;
    private readonly CreateCommentRequestValidator _validator;

    public CreateCommentRequestValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _commentRepository = _unitOfWork.Repositories.GetGeneric<Comment>();
        _mockCommentValidator = new Mock<IValidator<CommentCreateDto>>();
        _validator = new CreateCommentRequestValidator(
            _commentRepository,
            _mockCommentValidator.Object);
    }

    [Fact]
    public async Task Should_HaveError_When_CommentIsNull()
    {
        var request = new CreateCommentRequest
        {
            Comment = null,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Comment is required.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_CommentIsValid()
    {
        var request = new CreateCommentRequest
        {
            Comment = new CommentCreateDto
            {
                Body = "This is a test comment.",
            },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Comment);

        _mockCommentValidator.VerifyValidateCalledOnce(request.Comment);
    }

    [Fact]
    public async Task Should_HaveError_When_ParentCommentIdDoesNotExist()
    {
        var request = new CreateCommentRequest
        {
            ParentId = Guid.NewGuid(),
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ParentId)
            .WithErrorMessage($"Parent comment with Id {request.ParentId} does not exist.");
    }

    [Fact]
    public async Task Should_HaveError_When_ParentCommentIsNullAndActionIsSpecified()
    {
        var request = new CreateCommentRequest
        {
            ParentId = null,
            Action = CommentAction.Reply,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ParentId)
            .WithErrorMessage("Parent comment is required when action is specified.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_ParentCommentIdExists()
    {
        var existingParentComment = await SeedCommentAsync();
        var request = new CreateCommentRequest
        {
            ParentId = existingParentComment.Id,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.ParentId);
    }

    [Fact]
    public async Task Should_NotHaveError_When_ParentCommentIdIsEmpty()
    {
        var request = new CreateCommentRequest
        {
            ParentId = null,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.ParentId);
    }

    [Fact]
    public async Task Should_HaveError_When_ActionIsInvalid()
    {
        var request = new CreateCommentRequest
        {
            Action = (CommentAction)999,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Action)
            .WithErrorMessage("Invalid action specified.");
    }

    [Fact]
    public async Task Should_HaveError_When_ActionIsNullAndParentCommentIsSpecified()
    {
        var existingParentComment = await SeedCommentAsync();
        var request = new CreateCommentRequest
        {
            ParentId = existingParentComment.Id,
            Action = null,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Action)
            .WithErrorMessage("Action is required when parent comment is specified.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_ActionIsValid()
    {
        var request = new CreateCommentRequest
        {
            Action = CommentAction.Reply,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public async Task Should_NotHaveError_When_ActionIsNull()
    {
        var request = new CreateCommentRequest
        {
            Action = null,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    private async Task<Comment> SeedCommentAsync()
    {
        var comment = CommentTestData.GetComment();
        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return comment;
    }
}