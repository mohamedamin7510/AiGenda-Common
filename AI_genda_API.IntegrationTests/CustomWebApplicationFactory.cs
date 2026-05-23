using System;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using AI_genda_API.Entities;
using AI_genda_API.Presistience;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AI_genda_API.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public MockHttpMessageHandler MockHttpMessageHandler { get; } = new MockHttpMessageHandler();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<AI_genda_API.Presistience.AppContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AI_genda_API.Presistience.AppContext) ||
                d.ServiceType.Name.Contains("SqlServer") ||
                (d.ImplementationType != null && d.ImplementationType.Name.Contains("SqlServer")) ||
                d.ServiceType.Name.Contains("DbContextOptions")
            ).ToList();

            foreach(var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Mock Configuration payload specifically tracking GitHub/Google secrets
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"OAuth:GitHub:ClientId", "mock_gh_client"},
                    {"OAuth:GitHub:ClientSecret", "mock_gh_client_secret"},
                    {"OAuth:GitHub:RedirectUri", "https://localhost/callback"},
                    {"Authentication:GitHub:ClientId", "mock_gh_client"},
                    {"Authentication:GitHub:ClientSecret", "mock_gh_secret"},
                    {"Authentication:Google:ClientId", "mock_gg_client"},
                    {"Authentication:Google:ClientSecret", "mock_gg_secret"}
                });
            });

            services.AddDbContext<AI_genda_API.Presistience.AppContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
                options.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Ensure Auth handler replaces Default schemes cleanly
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, options => { });

            // Intercept HttpClient named "IntegrationClient" logic pipeline mappings overriding inner pipeline mock executions natively.
            services.Configure<HttpClientFactoryOptions>("IntegrationClient", options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    // Map Primary handler explicitly overriding standard SocketsHttpHandler
                    builder.PrimaryHandler = MockHttpMessageHandler;
                });
            });
        });
    }

    public void SeedDatabase(AI_genda_API.Presistience.AppContext context, Action<AI_genda_API.Presistience.AppContext> seeder)
    {
        seeder(context);
        context.SaveChanges();
    }
}