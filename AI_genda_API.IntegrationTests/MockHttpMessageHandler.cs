using System.Net;
using System.Net.Http.Headers;

namespace AI_genda_API.IntegrationTests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public AuthenticationHeaderValue? LastAuthorization { get; private set; }
    public string MockResponseContent { get; set; } = "{}";
    public HttpStatusCode MockResponseStatusCode { get; set; } = HttpStatusCode.OK;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastAuthorization = request.Headers.Authorization;

        var response = new HttpResponseMessage(MockResponseStatusCode)
        {
            Content = new StringContent(MockResponseContent, System.Text.Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);
    }
}
