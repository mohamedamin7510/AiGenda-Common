using System.Collections.Concurrent;
using System.Net.Http.Headers;
using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Exceptions;
using AI_genda_API.Services.AppConnectionService;

namespace AI_genda_API.Middlewares;

public class OAuthTokenRefreshHandler : DelegatingHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // We use a concurrent dictionary mapping provider to SemaphoreSlim 
    // to strictly prevent concurrent requests from identical users triggering parallel refresh logic.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public OAuthTokenRefreshHandler(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Establish dependencies within an explicit scope because HTTPClient limits bypass standard DI scopes natively.
        using var scope = _serviceScopeFactory.CreateScope();
        var appConnectionService = scope.ServiceProvider.GetRequiredService<IAppConnectionService>();

        // 2. Identify caller user details dynamically safely
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return await base.SendAsync(request, cancellationToken);

        // Assuming integration endpoints pass provider context using Request Items or HttpContext Extensions mapping route limits
        if (!httpContext.Items.TryGetValue("ActiveProvider", out var providerObj) || providerObj is not AppProvider provider)
            return await base.SendAsync(request, cancellationToken);

        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return await base.SendAsync(request, cancellationToken);

        // 3. Prevent Concurrent Token Exhaustion utilizing thread-safe locks locked strictly by distinct User + Provider limits
        var lockKey = $"{userId}_{provider}";
        var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        string? activeAccessToken;

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var connectionsListResult = await appConnectionService.GetConnectionsAsync(userId, cancellationToken);

            if (!connectionsListResult.IsSuccess || connectionsListResult.Value == null)
                throw new IntegrationMissingException($"No connection active found.");

            // Extract the AppConnection detail for standard payload
            var activeConnectionIdentifier = connectionsListResult.Value.FirstOrDefault(x => x.Provider == provider);

            if (activeConnectionIdentifier == null || !activeConnectionIdentifier.IsActive)
                throw new IntegrationMissingException($"Integration for {provider} is not active or set up.");

            // Detailed extraction for TokenExpiresAt evaluation mapping correctly
            var connectionDetailResult = await appConnectionService.GetConnectionAsync(activeConnectionIdentifier.Id, userId, cancellationToken);

            if (!connectionDetailResult.IsSuccess || connectionDetailResult.Value == null)
                throw new IntegrationMissingException("Connection payload failed validation execution.");

            // Guardrail execution: Token evaluates to expiring soon (< 5 minutes) requiring safe refresh
            if (connectionDetailResult.Value.TokenExpiresAt.HasValue && connectionDetailResult.Value.TokenExpiresAt.Value < DateTime.UtcNow.AddMinutes(5))
            {
                var connectorFactory = scope.ServiceProvider.GetRequiredService<IAppConnectorFactory>();
                var connector = connectorFactory.CreateConnector(provider);

                // Directly delegating down to connector triggers the secure DB swap utilizing EF mapped transparent encryption parameters implicitly
                // _Because AppConnectionService doesn't natively expose just the generic refresh proxy we use the factory directly_
                var dbContext = scope.ServiceProvider.GetRequiredService<Presistience.AppContext>();
                var rawDbConnection = await dbContext.AppConnections.FindAsync(new object[] { activeConnectionIdentifier.Id }, cancellationToken);

                if (rawDbConnection == null || string.IsNullOrEmpty(rawDbConnection.RefreshToken))
                    throw new IntegrationUnauthorizedException("Refresh token is explicitly missing. User must re-authenticate.");

                // Triggers native external provider execution
                var refreshedTokens = await connector.RefreshTokenAsync(rawDbConnection.RefreshToken, cancellationToken);

                rawDbConnection.AccessToken = refreshedTokens.AccessToken;
                rawDbConnection.RefreshToken = refreshedTokens.RefreshToken ?? rawDbConnection.RefreshToken;
                rawDbConnection.TokenExpiresAt = refreshedTokens.ExpiresAt;

                dbContext.AppConnections.Update(rawDbConnection);
                await dbContext.SaveChangesAsync(cancellationToken);

                activeAccessToken = refreshedTokens.AccessToken;
            }
            else
            {
                // Decrypt existing utilizing existing context
                var dbContext = scope.ServiceProvider.GetRequiredService<Presistience.AppContext>();
                var rawDbConnection = await dbContext.AppConnections.FindAsync(new object[] { activeConnectionIdentifier.Id }, cancellationToken);
                activeAccessToken = rawDbConnection?.AccessToken;
            }
        }
        finally
        {
            semaphore.Release();
        }

        // 4. Overwrite HttpRequestMessage globally mapping exactly to downstream Bearer integrations
        if (!string.IsNullOrEmpty(activeAccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", activeAccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
