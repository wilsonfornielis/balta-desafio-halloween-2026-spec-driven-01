using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Api.Application;
using Generator = PasswordGenerator.Api.Application.PasswordGenerator;

namespace PasswordGenerator.Tests.Integration;

public class PasswordEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_WithoutBody_Returns201AndDefaultLengthPassword()
    {
        var response = await _client.PostAsync("/passwords", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatePasswordResponse>();
        Assert.NotNull(created);
        Assert.Equal($"/passwords/{created.Id}", response.Headers.Location?.OriginalString);

        var password = await GetPasswordAsync(created.Id);
        Assert.Equal(16, password.Length);
        Assert.Contains(password, char.IsUpper);
        Assert.Contains(password, char.IsLower);
        Assert.Contains(password, char.IsDigit);
        Assert.Contains(password, c => Generator.Specials.Contains(c));
        Assert.DoesNotContain(password, char.IsWhiteSpace);
    }

    [Fact]
    public async Task Post_WithLength32_GeneratesPasswordWith32Characters()
    {
        var response = await _client.PostAsJsonAsync("/passwords", new CreatePasswordRequest(32));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatePasswordResponse>();
        Assert.Equal(32, (await GetPasswordAsync(created!.Id)).Length);
    }

    [Fact]
    public async Task Post_WithLength10_Returns400ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync("/passwords", new CreatePasswordRequest(10));

        await AssertValidationProblemAsync(response, "length");
    }

    [Fact]
    public async Task Post_WithMalformedJson_Returns400ProblemDetails()
    {
        var content = new StringContent("{x", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/passwords", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_ExistingId_Returns200WithSamePassword()
    {
        var created = await (await _client.PostAsync("/passwords", null)).Content.ReadFromJsonAsync<CreatePasswordResponse>();

        var first = await _client.GetFromJsonAsync<PasswordResponse>($"/passwords/{created!.Id}");
        var second = await _client.GetFromJsonAsync<PasswordResponse>($"/passwords/{created.Id}");

        Assert.NotNull(first);
        Assert.Equal(created.Id, first.Id);
        Assert.Equal(first.Password, second!.Password);
    }

    [Fact]
    public async Task Get_UnknownId_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"/passwords/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_InvalidGuid_Returns400ProblemDetails()
    {
        var response = await _client.GetAsync("/passwords/abc");

        await AssertValidationProblemAsync(response, "id");
    }

    private async Task<string> GetPasswordAsync(Guid id)
    {
        var password = await _client.GetFromJsonAsync<PasswordResponse>($"/passwords/{id}");
        Assert.NotNull(password);
        return password.Password;
    }

    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.ContainsKey(field));
    }
}
