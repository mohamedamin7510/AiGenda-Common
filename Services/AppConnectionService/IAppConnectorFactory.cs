using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Services.AppConnectionService.Connectors;

namespace AI_genda_API.Services.AppConnectionService;

/// <summary>
/// Factory for creating app connectors based on provider type
/// Extensible for adding new providers (GitHub, Notion, etc.) in the future
/// </summary>
public interface IAppConnectorFactory
{
    /// <summary>
    /// Creates the appropriate connector for the given provider
    /// </summary>
    IAppConnector CreateConnector(AppProvider provider);
}

/// <summary>
/// Implementation of app connector factory
/// Centralized location for registering new providers
/// 
/// To add a new provider:
/// 1. Create new connector class (e.g., GitHubConnector : IAppConnector)
/// 2. Add case in CreateConnector switch statement
/// 3. Update AppProvider enum
/// 4. That's it - rest of the app remains unchanged!
/// </summary>
public class AppConnectorFactory : IAppConnectorFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public AppConnectorFactory(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates the appropriate connector for the provider
    /// 
    /// Example - Adding GitHub:
    /// case AppProvider.GitHub:
    ///     return new GitHubConnector(_httpClientFactory.CreateClient(), _configuration, _loggerFactory.CreateLogger<GitHubConnector>());
    /// </summary>
    public IAppConnector CreateConnector(AppProvider provider)
    {
        return provider switch
        {
            AppProvider.Google => new GoogleCalendarConnector(
                _httpClientFactory.CreateClient(),
                _configuration,
                _loggerFactory.CreateLogger<GoogleCalendarConnector>()),

            AppProvider.GitHub => new GitHubConnector(
                _httpClientFactory.CreateClient("GitHubConnector"),
                _configuration,
                _loggerFactory.CreateLogger<GitHubConnector>()),

            // Future providers - add here
            // AppProvider.Notion => new NotionConnector(...),
            // AppProvider.TickTick => new TickTickConnector(...),
            // AppProvider.Microsoft => new MicrosoftConnector(...),
            // AppProvider.Slack => new SlackConnector(...),

            _ => throw new ArgumentException($"Unknown provider: {provider}")
        };
    }
}
