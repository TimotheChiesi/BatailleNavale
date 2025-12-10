namespace WebAppBackend;

using FluentValidation;
using Models;
using WebAppBackend.Services;

public class AttackRequestValidator : AbstractValidator<AttackRequest>
{
    public AttackRequestValidator(GameService gameService)
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("GameId must be provided.")
            .Must(id => gameService.GetGame(id) != null).WithMessage("Game not found.");

        When(x => gameService.GetGame(x.GameId) != null, () =>
        {
            RuleFor(x => x.Row)
                .GreaterThanOrEqualTo(0).WithMessage("Row must be a non-negative number.")
                .Must((req, row) =>
                {
                    var game = gameService.GetGame(req.GameId);
                    return row < game!.GridSize;
                }).WithMessage((req, row) => $"Row must be less than the grid size ({gameService.GetGame(req.GameId)!.GridSize}).");

            RuleFor(x => x.Col)
                .GreaterThanOrEqualTo(0).WithMessage("Col must be a non-negative number.")
                .Must((req, col) =>
                {
                    var game = gameService.GetGame(req.GameId);
                    return col < game!.GridSize;
                }).WithMessage((req, col) => $"Col must be less than the grid size ({gameService.GetGame(req.GameId)!.GridSize}).");
        });
    } 
}
