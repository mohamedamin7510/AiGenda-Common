using System.Net.Http.Json;
using System.Text.Json;
using AI_genda_API.Contracts.Integrations.Gmail;
using AI_genda_API.Exceptions;
using MimeKit;
using System.Text;

namespace AI_genda_API.Services.GmailIntegrationService;

public class GmailIntegrationService : IGmailIntegrationService
{
    private readonly HttpClient _httpClient;

    public GmailIntegrationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("IntegrationClient");
        _httpClient.BaseAddress = new Uri("https://gmail.googleapis.com/");
    }

    public async Task<object> GetInboxAsync(int maxResults = 10, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"gmail/v1/users/me/messages?maxResults={maxResults}&q=in:inbox", cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> GetMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"gmail/v1/users/me/messages/{messageId}", cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> SendMessageAsync(GmailSendRequest request, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
        message.Body = new TextPart("plain") { Text = request.BodyText };

        var rawMessage = EncodeMimeMessage(message);
        var payload = new { raw = rawMessage };

        var response = await _httpClient.PostAsJsonAsync("gmail/v1/users/me/messages/send", payload, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> ReplyToMessageAsync(GmailReplyRequest request, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
        message.Body = new TextPart("plain") { Text = request.BodyText };
        message.InReplyTo = request.OriginalMessageId;
        message.References.Add(request.OriginalMessageId);

        var rawMessage = EncodeMimeMessage(message);
        var payload = new { raw = rawMessage, threadId = request.ThreadId };

        var response = await _httpClient.PostAsJsonAsync("gmail/v1/users/me/messages/send", payload, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    public async Task<object> CreateDraftAsync(GmailDraftRequest request, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
        message.Body = new TextPart("plain") { Text = request.BodyText };

        var rawMessage = EncodeMimeMessage(message);
        var payload = new { message = new { raw = rawMessage } };

        var response = await _httpClient.PostAsJsonAsync("gmail/v1/users/me/drafts", payload, cancellationToken);
        return await HandleResponseAsync(response, cancellationToken);
    }

    private string EncodeMimeMessage(MimeMessage message)
    {
        using var stream = new MemoryStream();
        message.WriteTo(stream);
        var bytes = stream.ToArray();
        return Base64UrlEncode(bytes);
    }

    private string Base64UrlEncode(byte[] input)
    {
        var base64 = Convert.ToBase64String(input);
        return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private async Task<object> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gmail API integration failed: {response.StatusCode} - {content}");
        }

        return JsonSerializer.Deserialize<object>(content)!;
    }
}
