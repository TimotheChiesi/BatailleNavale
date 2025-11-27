namespace WebAppBackend;

using FluentValidation;
using Models;

public class AttackRequestValidator : AbstractValidator<AttackRequest>
{
    public AttackRequestValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("GameId must be provided.");

        RuleFor(x => x.Row)
            .InclusiveBetween(0, 9)
            .WithMessage("Row must be between 0 and 9.");

        RuleFor(x => x.Col)
            .InclusiveBetween(0, 9)
            .WithMessage("Col must be between 0 and 9.");
    }
}
