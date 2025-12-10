namespace WebAppBackend;

using FluentValidation;
using Models;
using WebAppBackend.Services;

public class FinalizePlacementRequestValidator : AbstractValidator<FinalizePlacementRequest>
{
    public FinalizePlacementRequestValidator(GameService gameService)
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("GameId must be provided.")
            .Must(id => gameService.GetGame(id) != null).WithMessage("Game not found.");

        RuleFor(x => x.Ships)
            .NotEmpty().WithMessage("At least one ship must be provided for placement.");

        RuleFor(x => x.Difficulty)
            .IsInEnum().WithMessage("A valid AI difficulty must be selected.");

        RuleFor(x => x.GridSize)
            .GreaterThan(0).WithMessage("Grid size must be a positive number.");
    }
}
