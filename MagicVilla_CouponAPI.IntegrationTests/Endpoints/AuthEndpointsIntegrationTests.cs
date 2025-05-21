using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MagicVilla_CouponAPI.IntegrationTests.Endpoints;

public class AuthEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserNameIsNotUnique()
    {
        var registration = new RegistrationRequestDTO
        {
            UserName = "existinguser",
            Name = "Test User",
            Password = "password"
        };

        // First registration should succeed CTL
        var firstResponse = await _client.PostAsJsonAsync("/api/register/", registration);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Second registration with same username should fail
        var secondResponse = await _client.PostAsJsonAsync("/api/register/", registration);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var apiResponse = await secondResponse.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse.Errors.Should().Contain("User name already exists");
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        var registration = new RegistrationRequestDTO
        {
            UserName = "loginuser",
            Name = "Login User",
            Password = "password"
        };

        // Register the user
        var regResponse = await _client.PostAsJsonAsync("/api/register/", registration);
        regResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = new LoginRequestDTO
        {
            UserName = "loginuser",
            Password = "password"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/login/", login);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var apiResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse.isSuccess.Should().BeTrue();
        apiResponse.Result.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
    {
        var login = new LoginRequestDTO
        {
            UserName = "nonexistent",
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/login/", login);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse.Errors.Should().Contain("User and/or password is incorrect");
    }
}