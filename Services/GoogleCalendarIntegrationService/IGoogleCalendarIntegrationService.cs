using AI_genda_API.Contracts.Integrations.GoogleCalendar;

namespace AI_genda_API.Services.GoogleCalendarIntegrationService;

public interface IGoogleCalendarIntegrationService
{
    Task<object> GetEventsAsync(DateTime? timeMin = null, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<object> CreateEventAsync(CalendarEventCreateRequest request, CancellationToken cancellationToken = default);
    Task<object> UpdateEventAsync(string eventId, CalendarEventUpdateRequest request, CancellationToken cancellationToken = default);
    Task<object> DeleteEventAsync(string eventId, CancellationToken cancellationToken = default);
}
