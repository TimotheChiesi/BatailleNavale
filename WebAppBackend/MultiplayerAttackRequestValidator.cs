using FluentValidation;
using Models;

namespace WebAppBackend;

public class MultiplayerAttackRequestValidator: AbstractValidator<MultiplayerAttackRequest>
{
    public MultiplayerAttackRequestValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("GameId must be provided.");

        RuleFor(x => x.Row)
            .InclusiveBetween(0, 9)
            .WithMessage("Row must be between 0 and 9.");

        RuleFor(x => x.Col)
            .InclusiveBetween(0, 9)
            .WithMessage("Col must be between 0 and 9.");
        
        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("PlayerId must be provided.");
    }
}