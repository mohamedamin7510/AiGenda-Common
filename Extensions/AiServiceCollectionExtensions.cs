using AI_genda_API.Services.Ai;
using AI_genda_API.Settings;
using Microsoft.Extensions.Options;

namespace AI_genda_API.Extensions;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiServiceSettings>()
            .Bind(configuration.GetSection(AiServiceSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IAiService, AiService>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<AiServiceSettings>>().Value;
            if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                client.BaseAddress = new Uri(settings.BaseUrl);
            }

            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        return services;
    }
}
