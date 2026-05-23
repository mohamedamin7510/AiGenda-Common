using AI_genda_API.Contracts.Integrations.Gmail;

namespace AI_genda_API.Services.GmailIntegrationService;

public interface IGmailIntegrationService
{
    Task<object> GetInboxAsync(int maxResults = 10, CancellationToken cancellationToken = default);
    Task<object> GetMessageAsync(string messageId, CancellationToken cancellationToken = default);
    Task<object> SendMessageAsync(GmailSendRequest request, CancellationToken cancellationToken = default);
    Task<object> ReplyToMessageAsync(GmailReplyRequest request, CancellationToken cancellationToken = default);
    Task<object> CreateDraftAsync(GmailDraftRequest request, CancellationToken cancellationToken = default);
}
