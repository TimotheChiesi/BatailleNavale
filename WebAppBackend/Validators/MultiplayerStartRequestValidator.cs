namespace WebAppBackend;

using FluentValidation;
using Models;
using WebAppBackend.Services;

public class MultiplayerStartRequestValidator : AbstractValidator<MultiplayerStartRequest>
{
    public MultiplayerStartRequestValidator(GameService gameService)
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId must be provided.")
            .Must(id => gameService.GetMultiplayerGame(id) is MultiplayerGameState).WithMessage("Game not found. Did the lobby start it correctly?");

        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("PlayerId must be provided.");

        When(x => !string.IsNullOrEmpty(x.RoomId) && !string.IsNullOrEmpty(x.PlayerId) && gameService.GetMultiplayerGame(x.RoomId) is MultiplayerGameState, () => 
        {
            RuleFor(x => x.PlayerId)
                .Must((req, playerId) =>
                {
                    var game = gameService.GetMultiplayerGame(req.RoomId) as MultiplayerGameState;
                    return playerId == game!.Player1Id || playerId == game.Player2Id;
                }).WithMessage("Player ID not found in this match.");
        });
    }
}
