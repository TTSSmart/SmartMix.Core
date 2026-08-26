using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartMix.Core.API;
using SmartMix.Core.Contracts.BaseModels;
using Xunit;

namespace SmartMix.Core.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override services for testing if needed
            });
        });
        
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Health_Ready_ReturnsReady()
    {
        var response = await _client.GetAsync("/health/ready");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Health_Live_ReturnsOk()
    {
        var response = await _client.GetAsync("/health/live");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Swagger_Endpoint_ReturnsJson()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
    
    [Fact]
    public async Task Auth_Login_WithoutCredentials_ReturnsUnauthorized()
    {
        var request = new AuthenticateRequest { Username = "test", Password = "wrong" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        
        // Without proper user setup, this will fail - but shouldn't crash
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Batching_Start_WithoutAuth_ReturnsUnauthorized()
    {
        var request = new StartBatchRequest { ApplicationId = 1, MixerNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/v1/batching/start", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Recipes_Get_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/recipes");
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database and PLC services
            // Add in-memory/test implementations
        });
        
        builder.UseEnvironment("Testing");
    }
}