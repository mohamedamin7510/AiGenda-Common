using AI_genda_API.Contracts.Integrations.GitHub;

namespace AI_genda_API.Services.GitHubIntegrationService;

public interface IGitHubIntegrationService
{
    Task<object> GetIssuesAsync(string state = "open", CancellationToken cancellationToken = default);
    Task<object> GetPullRequestsAsync(CancellationToken cancellationToken = default);
    Task<object> CreateIssueAsync(GitHubCreateIssueRequest request, CancellationToken cancellationToken = default);
    Task<object> CloseIssueAsync(GitHubCloseIssueRequest request, CancellationToken cancellationToken = default);
    Task<object> GetRepositoriesAsync(CancellationToken cancellationToken = default);
}
