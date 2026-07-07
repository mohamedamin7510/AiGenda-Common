using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using AI_genda_API.Contracts.Ai;

namespace AI_genda_API.Services.Ai;

public class AiService(HttpClient httpClient) : IAiService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<ChatResponse> ChatAsync(string userId, ChatRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Id", userId);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: cancellationToken);
        return payload ?? new ChatResponse(string.Empty);
    }

    public async IAsyncEnumerable<string> StreamChatAsync(string userId, ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat/stream")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Id", userId);

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                continue;
            }

            if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var data = line[5..].Trim();
                if (data.Length > 0)
                {
                    yield return data;
                }
            }
        }
    }

    public async Task<StatusResponse> GetStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/status");
        httpRequest.Headers.Add("X-User-Id", userId);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<StatusResponse>(cancellationToken: cancellationToken);
        return payload ?? new StatusResponse(string.Empty, string.Empty);
    }

    public async Task<WelcomeResponse> GetWelcomeAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/welcome");
        httpRequest.Headers.Add("X-User-Id", userId);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<WelcomeResponse>(cancellationToken: cancellationToken);
        return payload ?? new WelcomeResponse(string.Empty);
    }

    public async Task<AgentTreeRequest> BuildAgentTreeAsync(string userId, AgentTreeRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/tree")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-User-Id", userId);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AgentTreeRequest>(cancellationToken: cancellationToken);
        return payload ?? request;
    }

}
