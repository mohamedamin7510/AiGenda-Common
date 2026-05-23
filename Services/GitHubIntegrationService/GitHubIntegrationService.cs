using System.Net.Http.Json;
using System.Text.Json;
using AI_genda_API.Contracts.Integrations.GitHub;
using AI_genda_API.Exceptions;

namespace AI_genda_API.Services.GitHubIntegrationService;

public class GitHubIntegrationService : IGitHubIntegrationService
{
    private readonly HttpClient _httpClient;

    public GitHubIntegrationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("IntegrationClient");
        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AI-genda-API");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    public async Task<object> GetIssuesAsync(string state = "open", CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"issues?state={state}", cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> GetPullRequestsAsync(CancellationToken cancellationToken = default)
    {
        // GitHub considers PRs as issues, you can get search/issues
        var response = await _httpClient.GetAsync($"search/issues?q=is:pr+is:open+author:@me", cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> CreateIssueAsync(GitHubCreateIssueRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new { title = request.Title, body = request.Body };
        var response = await _httpClient.PostAsJsonAsync($"repos/{request.RepoOwner}/{request.RepoName}/issues", payload, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> CloseIssueAsync(GitHubCloseIssueRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new { state = "closed" };
        var response = await _httpClient.PatchAsJsonAsync($"repos/{request.RepoOwner}/{request.RepoName}/issues/{request.IssueNumber}", payload, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("user/repos?sort=updated", cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    private async Task<object> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new IntegrationExceptionHandler(response.StatusCode, content);
        }

        return JsonSerializer.Deserialize<object>(content)!;
    }
}

public class IntegrationExceptionHandler : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }
    public string Details { get; }

    public IntegrationExceptionHandler(System.Net.HttpStatusCode statusCode, string details)
        : base($"GitHub API request failed with status: {statusCode}")
    {
        StatusCode = statusCode;
        Details = details;
    }
}
