using FluentValidation;
using Gamestore.BLL.DTOs.Games.Comments;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Comments;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    private readonly IRepository<Comment> _commentRepository;

    public CreateCommentRequestValidator(
        IRepository<Comment> commentRepository,
        IValidator<CommentCreateDto> commentValidator)
    {
        _commentRepository = commentRepository;

        RuleFor(x => x.Comment)
            .NotNull().WithMessage("Comment is required.")
            .SetValidator(commentValidator);

        RuleFor(x => x)
            .MustAsync(BeValidParentIdAsync)
            .WithMessage(x => $"Parent comment with Id {x.ParentId} does not exist.")
            .OverridePropertyName(x => x.ParentId)
            .When(NotEmptyParentId);

        RuleFor(x => x.Action)
            .IsInEnum().WithMessage("Invalid action specified.")
            .When(NotEmptyAction);

        RuleFor(x => x)
            .Must(NotEmptyAction)
            .WithMessage("Action is required when parent comment is specified.")
            .OverridePropertyName(x => x.Action)
            .When(NotEmptyParentId);

        RuleFor(x => x)
            .Must(NotEmptyParentId)
            .WithMessage("Parent comment is required when action is specified.")
            .OverridePropertyName(x => x.ParentId)
            .When(NotEmptyAction);
    }

    private static bool NotEmptyParentId(CreateCommentRequest comment)
    {
        return comment.ParentId.HasValue;
    }

    private static bool NotEmptyAction(CreateCommentRequest comment)
    {
        return comment.Action.HasValue;
    }

    private Task<bool> BeValidParentIdAsync(CreateCommentRequest comment, CancellationToken cancellationToken)
    {
        return _commentRepository.ExistsAsync(
            c => c.Id == comment.ParentId,
            cancellationToken);
    }
}