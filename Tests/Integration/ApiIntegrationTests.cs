using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Models;
using Xunit;

namespace AdvisorySystem.Api.Tests.Integration;

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
      // Remove the app's DbContext registration
    var descriptor = services.SingleOrDefault(
          d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
      if (descriptor != null)
             services.Remove(descriptor);

 // Add in-memory database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
       options.UseInMemoryDatabase("TestDb");
                });

    // Build the service provider
                var sp = services.BuildServiceProvider();

    // Create a scope to seed the database
             using var scope = sp.CreateScope();
       var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();
    
    db.Database.EnsureCreated();
        // Seed test data here if needed
     });
        });

        _client = _factory.CreateClient();
    }

    #region Authentication Flow Tests

    [Fact]
    public async Task FullAuthenticationFlow_RegisterLoginRefresh_Success()
    {
        // 1. Register
        var registerDto = new
        {
       email = $"test{Guid.NewGuid()}@example.com",
            password = "Test@123456",
            fullName = "Test User"
        };

   var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // 2. Login
        var loginDto = new
    {
            email = registerDto.email,
  password = registerDto.password
     };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult?.Token);
    Assert.True(loginResult.ExpiresIn > 0);

        // 3. Access Protected Endpoint
        _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", loginResult.Token);

        var protectedResponse = await _client.GetAsync("/api/documents");
    Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);

      // 4. Refresh Token
     var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

   var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
    Assert.NotNull(refreshResult?.Token);
        Assert.NotEqual(loginResult.Token, refreshResult.Token);
    }

    #endregion

    #region Rate Limiting Tests

    [Fact]
    public async Task Login_ExceedsRateLimit_Returns429()
    {
   var loginDto = new
        {
         email = "test@example.com",
  password = "wrongpassword"
      };

     var responses = new List<HttpResponseMessage>();

        // Send 6 requests (limit is 5 per minute)
        for (int i = 0; i < 6; i++)
     {
   responses.Add(await _client.PostAsJsonAsync("/api/auth/login", loginDto));
        }

        // First 5 should be Unauthorized (wrong password)
     for (int i = 0; i < 5; i++)
    {
            Assert.Equal(HttpStatusCode.Unauthorized, responses[i].StatusCode);
        }

// 6th should be rate limited
   Assert.Equal(HttpStatusCode.TooManyRequests, responses[5].StatusCode);

 var errorResponse = await responses[5].Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("Too many requests", errorResponse?.Error);
        Assert.NotNull(errorResponse?.RetryAfter);
    }

    [Fact]
    public async Task Upload_ExceedsRateLimit_Returns429()
    {
// Login first to get token
   var token = await GetAuthToken();
      _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a document
     var createResponse = await _client.PostAsJsonAsync("/api/documents", new
        {
     title = "Test Document",
     tags = "test"
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateDocResponse>();

   // Try to upload 11 times (limit is 10 per minute)
      var responses = new List<HttpResponseMessage>();
     for (int i = 0; i < 11; i++)
        {
          var content = new MultipartFormDataContent();
          content.Add(new ByteArrayContent(new byte[1024]), "file", "test.pdf");
    content.Add(new StringContent("Test note"), "notes");

            responses.Add(await _client.PostAsync($"/api/documents/{createResult?.Id}/versions", content));
  }

 // Last request should be rate limited
    Assert.Equal(HttpStatusCode.TooManyRequests, responses[10].StatusCode);
    }

    [Fact]
    public async Task Search_SlidingWindow_RateLimitWorks()
    {
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Search has sliding window: 30 requests per minute with 6 segments (10 sec each)
        var responses = new List<HttpResponseMessage>();
        
        for (int i = 0; i < 35; i++)
        {
responses.Add(await _client.GetAsync("/api/search/documents?query=test"));
     
         // Add small delay every 10 requests to test sliding window
   if (i % 10 == 9)
            {
 await Task.Delay(500);
            }
  }

        // Should have some 429 responses
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.True(rateLimitedCount > 0, "Expected some requests to be rate limited");
    }

    #endregion

    #region Document Workflow Tests

    [Fact]
    public async Task DocumentWorkflow_CreateUploadDownload_Success()
    {
        var token = await GetAuthToken();
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Document
        var createResponse = await _client.PostAsJsonAsync("/api/documents", new
        {
     title = "Integration Test Document",
            tags = "test,integration"
        });
   Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateDocResponse>();
        Assert.NotNull(createResult?.Id);

    // 2. Upload Version
   var uploadContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("Test PDF Content"));
        uploadContent.Add(fileContent, "file", "test.pdf");
        uploadContent.Add(new StringContent("First version"), "notes");

        var uploadResponse = await _client.PostAsync(
            $"/api/documents/{createResult.Id}/versions", uploadContent);
  Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        // 3. Get Versions
        var versionsResponse = await _client.GetAsync($"/api/documents/{createResult.Id}/versions");
        Assert.Equal(HttpStatusCode.OK, versionsResponse.StatusCode);
   var versions = await versionsResponse.Content.ReadFromJsonAsync<List<VersionResponse>>();
   Assert.NotNull(versions);
        Assert.Single(versions);

   // 4. Download
        var downloadResponse = await _client.GetAsync($"/api/documents/download/{versions[0].Id}");
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
    Assert.Equal("application/octet-stream", downloadResponse.Content.Headers.ContentType?.MediaType);
    }

    #endregion

    #region CORS Tests

    [Fact]
    public async Task CORS_AllowedOrigin_ReturnsAccessControlHeaders()
    {
     _client.DefaultRequestHeaders.Add("Origin", "http://localhost:5173");
      
        var response = await _client.GetAsync("/api/courses");

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task CORS_DisallowedOrigin_NoAccessControlHeaders()
    {
        _client.DefaultRequestHeaders.Add("Origin", "http://evil.com");
        
        var response = await _client.GetAsync("/api/courses");

        // Should not have CORS headers for disallowed origins
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    #endregion

    #region Performance Tests

  [Fact]
    public async Task PerformanceTest_100ConcurrentRequests_CompletesInReasonableTime()
    {
  var token = await GetAuthToken();
        
        var tasks = new List<Task<HttpResponseMessage>>();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

 for (int i = 0; i < 100; i++)
        {
         var client = _factory.CreateClient();
   client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            tasks.Add(client.GetAsync("/api/courses"));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Should complete within 10 seconds
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
        $"100 concurrent requests took {stopwatch.ElapsedMilliseconds}ms");

        // Most requests should succeed
        var successCount = tasks.Count(t => t.Result.StatusCode == HttpStatusCode.OK);
        Assert.True(successCount > 50, $"Only {successCount}/100 requests succeeded");
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetAuthToken()
    {
        var email = $"test{Guid.NewGuid()}@example.com";
        
        // Register
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
        email,
            password = "Test@123456",
          fullName = "Test User"
  });

      // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
    {
    email,
     password = "Test@123456"
      });

        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return result?.Token ?? throw new Exception("Failed to get token");
    }

    #endregion

    #region Response Models

    private class LoginResponse
    {
        public string Token { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
  public int ExpiresIn { get; set; }
    }

    private class ErrorResponse
    {
        public string Error { get; set; } = "";
   public string Message { get; set; } = "";
        public double? RetryAfter { get; set; }
    }

    private class CreateDocResponse
    {
 public int Id { get; set; }
    }

    private class VersionResponse
    {
        public int Id { get; set; }
        public int VersionNo { get; set; }
        public string FileName { get; set; } = "";
    }

    #endregion
}
