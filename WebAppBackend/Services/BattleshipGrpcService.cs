using Grpc.Core;
using Models;
using FluentValidation;

namespace WebAppBackend.Services;

public class BattleshipGrpcService : BattleshipService.BattleshipServiceBase
{
    private readonly GameService _gameService;
    private readonly IValidator<AttackRequest> _validator;

    public BattleshipGrpcService(GameService gameService, IValidator<AttackRequest> validator)
    {
        _gameService = gameService;
        _validator = validator;
    }

    public override Task<AttackReplyGRPC> Attack(AttackRequestGRPC request, ServerCallContext context)
    {
    
        var gameId = Guid.TryParse(request.GameId, out var gId) ? gId : Guid.Empty;
        var game = _gameService.GetGame(gameId);
    
        if (game == null) 
        {
            return Task.FromResult(new AttackReplyGRPC { NotFound = "Game not found" });
        }

        var result = _gameService.Attack(game, request.Row, request.Col);

        AiAttackResultGRPC? aiGrpcResult = null;
    
        var lastAiAttack = result.AiAttackResults.LastOrDefault();
        if (lastAiAttack != null)
        {
            aiGrpcResult = new AiAttackResultGRPC
            {
                AiHit = lastAiAttack.AiAttackSucceeded,
                Row = lastAiAttack.Row,
                Col = lastAiAttack.Col
            };
        }

        return Task.FromResult(new AttackReplyGRPC
        {
            Ok = new AttackResponseGRPC
            {
                PlayerHit = result.PlayerAttackSucceeded,
                Winner = result.Winner ?? string.Empty,
                AiResult = aiGrpcResult 
            }
        });
    }
}