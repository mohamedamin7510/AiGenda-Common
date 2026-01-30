namespace AI_genda_API.Abstractions;

public record Error(string Code ,  string message)
{
    public static Error None = new Error(string.Empty, string.Empty);

}
