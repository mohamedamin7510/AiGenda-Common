using AI_genda_API.Contracts.Ai;

namespace AI_genda_API.Services.Ai;

public interface IAiPersistenceService
{
    Task<AgentTreeRequest> PersistAgentTreeAsync(string userId, AgentTreeRequest request, CancellationToken cancellationToken = default);

    Task<FactoryResetResponse> FactoryResetAsync(string userId, CancellationToken cancellationToken = default);
}
