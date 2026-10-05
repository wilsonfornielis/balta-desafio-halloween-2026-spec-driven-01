using System.Net;
using System.Text.Json;

namespace PasswordGenerator.Tests.Integration;

public class DocumentationEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/passwords", "post", new[] { "201", "400" })]
    [InlineData("/passwords/{id}", "get", new[] { "200", "400", "404" })]
    public async Task OpenApi_DescribesEndpointWithAllStatusCodes(string path, string method, string[] statusCodes)
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var operation = document.RootElement.GetProperty("paths").GetProperty(path).GetProperty(method);
        Assert.False(string.IsNullOrWhiteSpace(operation.GetProperty("summary").GetString()));

        var responses = operation.GetProperty("responses").EnumerateObject().Select(r => r.Name).ToArray();
        Assert.Equal(statusCodes.Order(), responses.Order());
    }

    [Fact]
    public async Task SwaggerUi_ReturnsHtmlPage()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("swagger-ui", await response.Content.ReadAsStringAsync());
    }
}
