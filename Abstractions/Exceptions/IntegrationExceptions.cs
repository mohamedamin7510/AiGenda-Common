namespace AI_genda_API.Exceptions;

public class IntegrationMissingException : Exception
{
    public IntegrationMissingException(string message = "Integration configuration is missing or inactive for this user.") : base(message)
    {
    }
}

public class IntegrationUnauthorizedException : Exception
{
    public IntegrationUnauthorizedException(string message = "Access token is invalid or expired.") : base(message)
    {
    }
}

public class IntegrationRateLimitException : Exception
{
    public IntegrationRateLimitException(string message = "Rate limit exceeded by the external provider.") : base(message)
    {
    }
}

public class IntegrationNotFoundException : Exception
{
    public IntegrationNotFoundException(string message = "Requested external resource was not found.") : base(message)
    {
    }
}