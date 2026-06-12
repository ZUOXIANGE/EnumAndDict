using System.Net.Http.Json;
using System.Text.Json;

namespace EnumDictDemo.Tests.Integration;

public class EnumsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EnumsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllEnums_ReturnsOkWithData()
    {
        var response = await _client.GetAsync("/api/enums", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var code = json.GetProperty("code").GetInt32();
        var data = json.GetProperty("data");

        Assert.Equal(200, code);
        Assert.True(data.TryGetProperty("OrderStatus", out _));
        Assert.True(data.TryGetProperty("PaymentMethod", out _));
    }

    [Fact]
    public async Task GetEnum_OrderStatus_ReturnsAllOptions()
    {
        var response = await _client.GetAsync("/api/enums/OrderStatus", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(200, json.GetProperty("code").GetInt32());

        var data = json.GetProperty("data");
        Assert.Equal(6, data.GetArrayLength());

        var first = data[0];
        Assert.True(first.TryGetProperty("value", out _));
        Assert.True(first.TryGetProperty("label", out _));
        Assert.True(first.TryGetProperty("name", out _));
    }

    [Fact]
    public async Task GetEnum_OrderStatusCaseInsensitive_ReturnsOptions()
    {
        var response = await _client.GetAsync("/api/enums/orderstatus", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(200, json.GetProperty("code").GetInt32());
    }

    [Fact]
    public async Task GetEnum_UnknownEnum_Returns404()
    {
        var response = await _client.GetAsync("/api/enums/NonExistent", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(404, json.GetProperty("code").GetInt32());
        Assert.Contains("not found", json.GetProperty("message").GetString());
    }
}
