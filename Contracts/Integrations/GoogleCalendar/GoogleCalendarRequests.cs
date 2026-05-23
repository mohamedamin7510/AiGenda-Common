using System.Text.Json.Serialization;

namespace AI_genda_API.Contracts.Integrations.GoogleCalendar;

public record GoogleCalendarDateTime(
    [property: JsonPropertyName("dateTime")] string DateTime,
    [property: JsonPropertyName("timeZone")] string TimeZone = "UTC"
);

public record CalendarEventCreateRequest(
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("start")] GoogleCalendarDateTime Start,
    [property: JsonPropertyName("end")] GoogleCalendarDateTime End
);

public record CalendarEventUpdateRequest(
    [property: JsonIgnore] string EventId,  // Used for route/identification, ignored in payload to Google
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("start")] GoogleCalendarDateTime Start,
    [property: JsonPropertyName("end")] GoogleCalendarDateTime End
);
