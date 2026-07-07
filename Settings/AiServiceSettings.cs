namespace AI_genda_API.Settings;

public class AiServiceSettings
{
    public const string SectionName = "AiServiceSettings";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}
