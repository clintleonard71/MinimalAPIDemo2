using FluentAssertions;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MagicVilla_CouponAPI.IntegrationTests.Endpoints;

public class CouponEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CouponEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllCoupons_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/coupons/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCoupon_ReturnsOk_AndCouponIsRetrievable()
    {
        var coupon = new CouponCreateDTO
        {
            Name = "INTEGRATION10",
            Percent = 10,
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/coupons/", coupon);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Optionally, check if the coupon is in the list
        var listResponse = await _client.GetAsync("/api/coupons/");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var coupons = await listResponse.Content.ReadFromJsonAsync<List<CouponDTO>>();
        coupons.Should().Contain(c => c.Name == "INTEGRATION10");
    }
}