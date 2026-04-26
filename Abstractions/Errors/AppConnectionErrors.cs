namespace AI_genda_API.Abstractions.Errors;

public static class AppConnectionErrors
{
    public static readonly Error ConnectionNotFound = new(
        "APP_CONNECTION_NOT_FOUND",
        "The app connection was not found",
        404
    );

    public static readonly Error ProviderNotSupported = new(
        "PROVIDER_NOT_SUPPORTED",
        "The specified app provider is not currently supported",
        400
    );

    public static readonly Error InvalidOAuthCode = new(
        "INVALID_OAUTH_CODE",
        "The OAuth authorization code is invalid or expired",
        400
    );

    public static readonly Error TokenRefreshFailed = new(
        "TOKEN_REFRESH_FAILED",
        "Failed to refresh the access token",
        500
    );

    public static readonly Error SyncFailed = new(
        "SYNC_FAILED",
        "Failed to sync data from the external provider",
        500
    );

    public static readonly Error InvalidToken = new(
        "INVALID_TOKEN",
        "The access token is invalid or expired",
        401
    );

    public static readonly Error ConnectionAlreadyExists = new(
        "CONNECTION_ALREADY_EXISTS",
        "A connection to this app provider already exists for this user",
        409
    );

    public static readonly Error UnauthorizedAccess = new(
        "UNAUTHORIZED_ACCESS",
        "You do not have permission to access this connection",
        403
    );

    public static readonly Error RevocationFailed = new(
        "REVOCATION_FAILED",
        "Failed to revoke access from the external provider",
        500
    );
}
