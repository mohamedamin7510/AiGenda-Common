namespace AI_genda_API.Contracts.Integrations.GitHub;

public record GitHubCreateIssueRequest(string RepoOwner, string RepoName, string Title, string Body);
public record GitHubCloseIssueRequest(string RepoOwner, string RepoName, int IssueNumber);
