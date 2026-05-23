using System.Net.Http.Json;
using System.Text.Json;
using AI_genda_API.Contracts.Integrations.GoogleCalendar;

namespace AI_genda_API.Services.GoogleCalendarIntegrationService;

public class GoogleCalendarIntegrationService : IGoogleCalendarIntegrationService
{
    private readonly HttpClient _httpClient;

    public GoogleCalendarIntegrationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("IntegrationClient");
        _httpClient.BaseAddress = new Uri("https://www.googleapis.com/");
    }

    public async Task<object> GetEventsAsync(DateTime? timeMin = null, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var timeMinParam = timeMin?.ToString("O") ?? DateTime.UtcNow.ToString("O");
        var url = $"calendar/v3/calendars/primary/events?maxResults={maxResults}&timeMin={Uri.EscapeDataString(timeMinParam)}&singleEvents=true&orderBy=startTime";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> CreateEventAsync(CalendarEventCreateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("calendar/v3/calendars/primary/events", request, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> UpdateEventAsync(string eventId, CalendarEventUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"calendar/v3/calendars/primary/events/{eventId}", request, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> DeleteEventAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"calendar/v3/calendars/primary/events/{eventId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Google Calendar API integration failed: {response.StatusCode} - {content}");
        }

        return new { status = "Event deleted successfully." };
    }

    private async Task<object> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Google Calendar API integration failed: {response.StatusCode} - {content}");
        }

        return JsonSerializer.Deserialize<object>(content)!;
    }
}
