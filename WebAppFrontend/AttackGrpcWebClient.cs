using Models;

namespace WebAppFrontend;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;


public class AttackGrpcWebClient
{
    // Use the generated client name based on the proto 'service' name
    private readonly BattleshipService.BattleshipServiceClient _client;

    public AttackGrpcWebClient(string baseUri)
    {
        // Setup gRPC-Web Handler (Required for Browser calls)
        var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
        
        var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        _client = new BattleshipService.BattleshipServiceClient(channel);
    }

    // Input: Domain Request | Output: Domain Response
    public async Task<AttackResponse?> SendAttackAsync(AttackRequest request)
    {
        // 1. Map Domain -> Proto Request
        var protoRequest = new AttackRequestGRPC
        {
            GameId = request.GameId.ToString(),
            Row = request.Row,
            Col = request.Col
        };

        try 
        {
            // 2. Make the Network Call
            var reply = await _client.AttackAsync(protoRequest);

            // 3. Handle the 'OneOf' result
            // The generated code has a "Case" enum to check which field was set
            switch (reply.ResultCase)
            {
                case AttackReplyGRPC.ResultOneofCase.Ok:
                    return MapToDomain(reply.Ok);

                case AttackReplyGRPC.ResultOneofCase.BadRequest:
                    Console.WriteLine("Validation Error: " + string.Join(", ", reply.BadRequest.Errors));
                    return null; // Or throw custom exception

                case AttackReplyGRPC.ResultOneofCase.NotFound:
                    Console.WriteLine("Error: " + reply.NotFound);
                    return null; // Or throw custom exception

                default:
                    Console.WriteLine("Unknown response type");
                    return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gRPC Transport Error: {ex.Message}");
            return null;
        }
    }

    private AttackResponse MapToDomain(AttackResponseGRPC grpcResponse)
    {
        // Map Proto Response -> Domain Response
        return new AttackResponse
        {
            PlayerAttackSucceeded = grpcResponse.PlayerHit,
            Winner = string.IsNullOrEmpty(grpcResponse.Winner) ? null : grpcResponse.Winner,
            
            // Map the nested AI result if it exists
            AiAttackResult = grpcResponse.AiResult == null ? null : new AiAttackResult
            {
                AiAttackSucceeded = grpcResponse.AiResult.AiHit,
                Row = grpcResponse.AiResult.Row,
                Col = grpcResponse.AiResult.Col
            }
        };
    }
}
