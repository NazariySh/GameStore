using FluentValidation;
using Gamestore.BLL.DTOs.Orders;

namespace Gamestore.BLL.Validators.Orders;

public class UpdateQuantityRequestValidator : AbstractValidator<UpdateQuantityRequest>
{
    public UpdateQuantityRequestValidator()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0).WithMessage("Count cannot be negative or zero.");
    }
}