using FluentValidation;
using Gamestore.BLL.DTOs.Games.Comments;

namespace Gamestore.BLL.Validators.Games.Comments;

public class CommentCreateDtoValidator : AbstractValidator<CommentCreateDto>
{
    public CommentCreateDtoValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.")
            .MaximumLength(CommentValidationRules.MaxBodyLength)
            .WithMessage($"Body must not exceed {CommentValidationRules.MaxBodyLength} characters.");
    }
}