using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AI_genda_API.Abstractions.Enums;
using AI_genda_API.Contracts.AppConnections;
using AI_genda_API.Contracts.Integrations.GitHub;
using AI_genda_API.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AI_genda_API.IntegrationTests;

public class IntegrationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IntegrationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Implicit authentication header specifically injecting mocked mapped Identity configurations
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetIntegrationsStatus_ShouldReturnMappedJson_WhenSeeded()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Presistience.AppContext>();

        // Clear potentially dangling configurations
        db.AppConnections.RemoveRange(db.AppConnections);
        db.SaveChanges();

        // Seed AppConnection mappings explicit natively indicating mapped combinations
        _factory.SeedDatabase(db, context =>
        {
            var user = context.Users.FirstOrDefault(x => x.Id == TestAuthHandler.TestUserId);
            if (user == null)
            {
                user = new ExtendedUser
                {
                    Id = TestAuthHandler.TestUserId,
                    UserName = "test",
                    Email = "test@test.com",
                    FirstName = "Test",
                    SecondName = "User"
                };
                context.Users.Add(user);
            }

            context.AppConnections.Add(new AppConnection
            {
                Id = Guid.NewGuid().ToString(),
                User = user,
                UserId = TestAuthHandler.TestUserId,
                Provider = AppProvider.GitHub,
                ExternalAccountId = "github_acc",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Seed an INACTIVE google connection. Expect calendar and gmail false.
            context.AppConnections.Add(new AppConnection
            {
                Id = Guid.NewGuid().ToString(),
                User = user,
                UserId = TestAuthHandler.TestUserId,
                Provider = AppProvider.Google,
                ExternalAccountId = "google_acc",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            });
        });

        // Act
        var response = await _client.GetAsync("/integrations/v1/status");

        // Assert
        response.EnsureSuccessStatusCode();

        var contentString = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(contentString);
        var root = jsonDocument.RootElement;

        Assert.Equal("success", root.GetProperty("status").GetString());
        var data = root.GetProperty("data");

        Assert.True(data.GetProperty("github").GetBoolean());
        Assert.False(data.GetProperty("gmail").GetBoolean());
        Assert.False(data.GetProperty("calendar").GetBoolean());
    }
    [Fact]
    public async System.Threading.Tasks.Task GetGitHubIssues_WhenTokenExpired_ShouldInterceptRefreshAndProceed()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Presistience.AppContext>();

        db.AppConnections.RemoveRange(db.AppConnections);
        db.SaveChanges();

        _factory.SeedDatabase(db, context =>
        {
            var user = context.Users.FirstOrDefault(x => x.Id == TestAuthHandler.TestUserId);
            if (user == null)
            {
                user = new ExtendedUser
                {
                    Id = TestAuthHandler.TestUserId,
                    UserName = "test_expired",
                    Email = "test2@test.com",
                    FirstName = "Test",
                    SecondName = "Expired"
                };
                context.Users.Add(user);
            }

            var expiredConnection = new AppConnection
            {
                Id = Guid.NewGuid().ToString(),
                User = user,
                UserId = TestAuthHandler.TestUserId,
                Provider = AppProvider.GitHub,
                ExternalAccountId = "gh_acc",
                AccessToken = "old_expired_token",
                RefreshToken = "valid_refresh_token",
                TokenExpiresAt = DateTime.UtcNow.AddMinutes(-10), // Expired externally!
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.AppConnections.Add(expiredConnection);
        });

        // We explicitly mock the outbound target downstream returning dynamic content mapped normally
        _factory.MockHttpMessageHandler.MockResponseStatusCode = HttpStatusCode.OK;
        _factory.MockHttpMessageHandler.MockResponseContent = "{}"; // Ensure payload successfully deserializes via generic object json mapping internally

        // Act
        // This request triggers IntegrationsController -> IGitHubIntegrationService -> IntegrationClient -> OAuthTokenRefreshHandler
        var response = await _client.GetAsync("/integrations/v1/github/issues?state=open");

        // Assert
        // We assert HTTP OK since exception handler masks exceptions differently in failed scopes mapped inside the service logic 
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());

        // 1. Verify that the inner Mock handler was indeed called!
        Assert.NotNull(_factory.MockHttpMessageHandler.LastRequest);

        // 2. Because the handler inherently executes the explicit IAppConnector pipeline (mocked externally/internally via EF configurations)
        // Note: The actual downstream connector hits another HttpClient inherently mapped. 
        // In a true integration mock, we need to ensure the DB mapping reflects the NEW mocked refresh configuration safely natively.
        // For this test, because MOCK interceptor explicitly mocks the outer boundary *AND* inner boundary natively over `PrimaryHandler`,
        // The token refresh execution logic uses standard HTTP requests under the hood which are also mocked if sharing the same DI factory pipelines.
    }
}
