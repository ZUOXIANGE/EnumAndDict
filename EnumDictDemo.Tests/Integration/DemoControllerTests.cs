using System.Net.Http.Json;
using System.Text.Json;

namespace EnumDictDemo.Tests.Integration;

public class DemoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DemoControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ===== GET /api/demo/user =====

    [Fact]
    public async Task GetUser_ReturnsTranslatedResponse()
    {
        var response = await _client.GetAsync("/api/demo/user", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(200, json.GetProperty("code").GetInt32());

        var user = json.GetProperty("data");
        Assert.Equal("张三", user.GetProperty("name").GetString());
        Assert.Equal("1", user.GetProperty("sex").GetString());
        Assert.Equal("男", user.GetProperty("sexDesc").GetString());
        Assert.Equal("3", user.GetProperty("nation").GetString());
        Assert.Equal("回族", user.GetProperty("nationDesc").GetString());
    }

    [Fact]
    public async Task GetUsers_ReturnsTranslatedList()
    {
        var response = await _client.GetAsync("/api/demo/users", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var data = json.GetProperty("data");
        Assert.Equal(3, data.GetArrayLength());

        foreach (var user in data.EnumerateArray())
        {
            Assert.True(user.TryGetProperty("sexDesc", out _));
            Assert.True(user.TryGetProperty("nationDesc", out _));
            Assert.NotEmpty(user.GetProperty("sexDesc").GetString()!);
            Assert.NotEmpty(user.GetProperty("nationDesc").GetString()!);
        }
    }

    // ===== GET /api/demo/order =====

    [Fact]
    public async Task GetOrder_ReturnsTranslatedWithNestedUser()
    {
        var response = await _client.GetAsync("/api/demo/order", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var order = json.GetProperty("data");

        // 订单字典翻译
        Assert.Equal("app", order.GetProperty("source").GetString());
        Assert.Equal("APP端", order.GetProperty("sourceDesc").GetString());

        // 订单枚举 Label
        Assert.NotEmpty(order.GetProperty("statusLabel").GetString()!);
        Assert.NotEmpty(order.GetProperty("paymentMethodLabel").GetString()!);

        // 嵌套 Buyer 字典翻译
        var buyer = order.GetProperty("buyer");
        Assert.Equal("1", buyer.GetProperty("sex").GetString());
        Assert.Equal("男", buyer.GetProperty("sexDesc").GetString());
        Assert.Equal("3", buyer.GetProperty("nation").GetString());
        Assert.Equal("回族", buyer.GetProperty("nationDesc").GetString());
    }

    [Fact]
    public async Task GetOrders_ReturnsTranslatedListWithNested()
    {
        var response = await _client.GetAsync("/api/demo/orders", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        var orders = json.GetProperty("data");
        Assert.Equal(3, orders.GetArrayLength());

        // 第一个订单有嵌套 Buyer
        var first = orders[0];
        Assert.Equal("APP端", first.GetProperty("sourceDesc").GetString());
        Assert.NotEqual(JsonValueKind.Null, first.GetProperty("buyer").ValueKind);
        Assert.Equal("张三", first.GetProperty("buyer").GetProperty("name").GetString());

        // 第三个订单 Buyer 为 null
        var third = orders[2];
        Assert.Equal("小程序", third.GetProperty("sourceDesc").GetString());
        Assert.Equal(JsonValueKind.Null, third.GetProperty("buyer").ValueKind);
    }

    // ===== POST /api/demo/validate-dict-value =====

    [Fact]
    public async Task ValidateDictValue_Valid_ReturnsOk()
    {
        var content = JsonContent.Create(new { dictCode = "sex", dictValue = "1" });
        var response = await _client.PostAsync("/api/demo/validate-dict-value", content, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(200, json.GetProperty("code").GetInt32());
        Assert.Equal("Valid", json.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ValidateDictValue_Invalid_ReturnsBadRequest()
    {
        var content = JsonContent.Create(new { dictCode = "sex", dictValue = "999" });
        var response = await _client.PostAsync("/api/demo/validate-dict-value", content, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(400, json.GetProperty("code").GetInt32());
    }

    // ===== POST /api/demo/validate-enum =====

    [Fact]
    public async Task ValidateEnum_ValidName_ReturnsOk()
    {
        var content = JsonContent.Create(new { enumName = "OrderStatus", enumValue = "Pending" });
        var response = await _client.PostAsync("/api/demo/validate-enum", content, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(200, json.GetProperty("code").GetInt32());
    }

    [Fact]
    public async Task ValidateEnum_ValidNumeric_ReturnsOk()
    {
        var content = JsonContent.Create(new { enumName = "OrderStatus", enumValue = "3" });
        var response = await _client.PostAsync("/api/demo/validate-enum", content, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(200, json.GetProperty("code").GetInt32());
        Assert.Equal("Completed", json.GetProperty("parsedValue").GetString());
    }

    [Fact]
    public async Task ValidateEnum_InvalidValue_ReturnsBadRequest()
    {
        var content = JsonContent.Create(new { enumName = "OrderStatus", enumValue = "InvalidStatus" });
        var response = await _client.PostAsync("/api/demo/validate-enum", content, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidateEnum_UnknownEnum_ReturnsBadRequest()
    {
        var content = JsonContent.Create(new { enumName = "NonExistent", enumValue = "Value" });
        var response = await _client.PostAsync("/api/demo/validate-enum", content, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ===== POST /api/demo/create-order =====

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreated()
    {
        var content = JsonContent.Create(new
        {
            orderNo = "ORD-TEST-001",
            amount = 199.99m,
            status = "Pending",
            paymentMethod = "WeChatPay",
            source = "app"
        });
        var response = await _client.PostAsync("/api/demo/create-order", content, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(201, json.GetProperty("code").GetInt32());

        var order = json.GetProperty("data");
        Assert.Equal("ORD-TEST-001", order.GetProperty("orderNo").GetString());
        Assert.Equal("APP端", order.GetProperty("sourceDesc").GetString());
    }

    [Fact]
    public async Task CreateOrder_InvalidSource_ReturnsBadRequest()
    {
        var content = JsonContent.Create(new
        {
            orderNo = "ORD-TEST-002",
            amount = 100m,
            status = "Pending",
            paymentMethod = "WeChatPay",
            source = "invalid_source"
        });
        var response = await _client.PostAsync("/api/demo/create-order", content, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ===== POST /api/demo/create-user =====

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        var content = JsonContent.Create(new
        {
            name = "测试用户",
            sex = "1",
            nation = "3",
            age = 25,
            email = "test@example.com"
        });
        var response = await _client.PostAsync("/api/demo/create-user", content, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(201, json.GetProperty("code").GetInt32());
        Assert.Equal("男", json.GetProperty("data").GetProperty("sexDesc").GetString());
        Assert.Equal("回族", json.GetProperty("data").GetProperty("nationDesc").GetString());
    }

    [Fact]
    public async Task CreateUser_InvalidSex_ReturnsBadRequest()
    {
        var content = JsonContent.Create(new
        {
            name = "测试用户",
            sex = "99",
            nation = "1",
            age = 25,
            email = "test@example.com"
        });
        var response = await _client.PostAsync("/api/demo/create-user", content, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_InvalidNation_ReturnsBadRequest()
    {
        var content = JsonContent.Create(new
        {
            name = "测试用户",
            sex = "1",
            nation = "99",
            age = 25,
            email = "test@example.com"
        });
        var response = await _client.PostAsync("/api/demo/create-user", content, TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ===== POST /api/demo/refresh-cache =====

    [Fact]
    public async Task RefreshCache_ReturnsOk()
    {
        var response = await _client.PostAsync("/api/demo/refresh-cache", null, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(200, json.GetProperty("code").GetInt32());
        Assert.Equal("Cache refreshed", json.GetProperty("message").GetString());
    }
}
