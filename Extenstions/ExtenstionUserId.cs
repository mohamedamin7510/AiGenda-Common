namespace AI_genda_API.Api.Extenstions;

public static class ExtenstionUserId
{
    public static string? GetUserId(this ClaimsPrincipal claimsPrincipal) =>
         claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

}
