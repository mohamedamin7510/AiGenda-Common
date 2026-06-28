using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Abstractions.Errors;
using AI_genda_API.Entities;
using AI_genda_API.Services.AppConnectionService;
using Microsoft.EntityFrameworkCore;

namespace AI_genda_API.Services.TokenManagement;

public class TokenManagerService(
    AppContext dbContext,
    ITokenEncryptionService encryptionService,
    IAppConnectorFactory connectorFactory,
    ILogger<TokenManagerService> logger) : ITokenManagerService
{
    private readonly AppContext _dbContext = dbContext;
    private readonly ITokenEncryptionService _encryptionService = encryptionService;
    private readonly IAppConnectorFactory _connectorFactory = connectorFactory;
    private readonly ILogger<TokenManagerService> _logger = logger;

    public async Task<Result<string>> GetValidAccessTokenAsync(string userId, AppProvider provider, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _dbContext.AppConnections
                .Where(c => c.UserId == userId && c.Provider == provider && c.IsActive)
                .SingleOrDefaultAsync(cancellationToken);

            if (connection == null)
            {
                _logger.LogWarning("No active {Provider} connection found for User {UserId}", provider, userId);
                return Result.Faluire<string>(new Error("CONNECTION_NOT_FOUND", $"No active connection found for {provider}", 404));
            }

            // قراءة النص صراحة وبدون فك تشفير
            var decryptedAccessToken = connection.AccessToken;

            if (connection.TokenExpiresAt.HasValue && connection.TokenExpiresAt.Value <= DateTime.UtcNow.AddMinutes(5))
            {
                if (string.IsNullOrEmpty(connection.RefreshToken))
                {
                    _logger.LogWarning("Token for {Provider} (User: {UserId}) is expiring and NO refresh token is stored.", provider, userId);
                    return Result.Faluire<string>(new Error("REFRESH_TOKEN_MISSING", "Token is expired and no refresh token is available.", 401));
                }

                _logger.LogInformation("Token for {Provider} (User: {UserId}) is expiring. Refreshing silently for AI Agent.", provider, userId);

                try
                {
                    var decryptedRefreshToken = connection.RefreshToken;
                    var connector = _connectorFactory.CreateConnector(provider);

                    var refreshResponse = await connector.RefreshTokenAsync(decryptedRefreshToken, cancellationToken);

                    decryptedAccessToken = refreshResponse.AccessToken;
                    connection.AccessToken = refreshResponse.AccessToken;

                    if (!string.IsNullOrEmpty(refreshResponse.RefreshToken))
                    {
                        connection.RefreshToken = refreshResponse.RefreshToken;
                    }

                    connection.TokenExpiresAt = refreshResponse.ExpiresAt;
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Successfully refreshed token for {Provider} (User: {UserId}).", provider, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to silently refresh token for {Provider} (User: {UserId}). External API issue.", provider, userId);
                    return Result.Faluire<string>(new Error("TOKEN_REFRESH_FAILED", "Could not refresh access token.", 500));
                }
            }

            return Result.Success(decryptedAccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve access token for {Provider} (User {UserId})", provider, userId);
            return Result.Faluire<string>(new Error("TOKEN_FETCH_ERROR", "Internal server error retrieving connection", 500));
        }
    }
}