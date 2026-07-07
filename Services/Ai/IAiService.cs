using AI_genda_API.Contracts.Ai;

namespace AI_genda_API.Services.Ai;

public interface IAiService
{
    Task<ChatResponse> ChatAsync(string userId, ChatRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> StreamChatAsync(string userId, ChatRequest request, CancellationToken cancellationToken = default);

    Task<StatusResponse> GetStatusAsync(string userId, CancellationToken cancellationToken = default);

    Task<WelcomeResponse> GetWelcomeAsync(string userId, CancellationToken cancellationToken = default);

    Task<AgentTreeRequest> BuildAgentTreeAsync(string userId, AgentTreeRequest request, CancellationToken cancellationToken = default);
}
