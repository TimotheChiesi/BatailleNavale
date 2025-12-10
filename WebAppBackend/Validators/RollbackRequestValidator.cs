namespace WebAppBackend;

using FluentValidation;
using Models;
using WebAppBackend.Services;

public class RollbackRequestValidator : AbstractValidator<RollbackRequest>
{
    public RollbackRequestValidator(GameService gameService)
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("GameId must be provided.")
            .Must(id => gameService.GetGame(id) != null).WithMessage("Game not found.");

        When(x => gameService.GetGame(x.GameId) != null, () =>
        {
            RuleFor(x => x.Index)
                .GreaterThanOrEqualTo(0).WithMessage("Index must be a non-negative number.")
                .Must((req, index) =>
                {
                    var game = gameService.GetGame(req.GameId);
                    return index < game!.History.Count;
                }).WithMessage((req, index) => $"Index must be less than the history count ({gameService.GetGame(req.GameId)!.History.Count}).");
        });
    }
}
