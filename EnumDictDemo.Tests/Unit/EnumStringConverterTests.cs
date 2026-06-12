using System.Text.Json;
using System.Text.Json.Serialization;
using EnumDictDemo.Models.Dto;
using FastEnumUtility;

namespace EnumDictDemo.Tests.Unit;

public class EnumStringConverterTests
{
    private readonly JsonSerializerOptions _options;

    public EnumStringConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverterWithFallback() }
        };
    }

    // 用一个辅助类包装转换器测试
    [Fact]
    public void Write_WritesEnumNameAsString()
    {
        var dto = new TestOrderDto { Status = TestOrderStatus.Shipped };
        var json = JsonSerializer.Serialize(dto, _options);

        Assert.Contains("\"Shipped\"", json);
    }

    [Fact]
    public void Read_ValidName_DeserializesCorrectly()
    {
        var json = """{"Status":"Completed"}""";

        var dto = JsonSerializer.Deserialize<TestOrderDto>(json, _options)!;

        Assert.Equal(TestOrderStatus.Completed, dto.Status);
    }

    [Fact]
    public void Read_ValidNameCaseInsensitive()
    {
        var json = """{"Status":"completed"}""";

        var dto = JsonSerializer.Deserialize<TestOrderDto>(json, _options)!;

        Assert.Equal(TestOrderStatus.Completed, dto.Status);
    }

    [Fact]
    public void Read_ValidNumericValue_DeserializesCorrectly()
    {
        var json = """{"Status":3}""";

        var dto = JsonSerializer.Deserialize<TestOrderDto>(json, _options)!;

        Assert.Equal(TestOrderStatus.Completed, dto.Status);
    }

    [Fact]
    public void Read_InvalidValue_ThrowsJsonException()
    {
        var json = """{"Status":"InvalidStatus"}""";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TestOrderDto>(json, _options));
    }

    [Fact]
    public void Write_ProducesValidRoundtrip()
    {
        var dto = new TestOrderDto { Status = TestOrderStatus.Cancelled };
        var json = JsonSerializer.Serialize(dto, _options);
        var result = JsonSerializer.Deserialize<TestOrderDto>(json, _options)!;

        Assert.Equal(TestOrderStatus.Cancelled, result.Status);
    }
}

// --- 测试辅助类 ---

public enum TestOrderStatus
{
    [Label("待支付")]
    Pending = 0,
    [Label("已支付")]
    Paid = 1,
    [Label("已发货")]
    Shipped = 2,
    [Label("已完成")]
    Completed = 3,
    [Label("已取消")]
    Cancelled = 4,
}

public class TestOrderDto
{
    [JsonConverter(typeof(EnumStringConverter<TestOrderStatus>))]
    public TestOrderStatus Status { get; set; }
}

/// <summary>
/// 辅助类，让 JsonStringEnumConverter 与 EnumStringConverter 并存时用于区分
/// </summary>
file class JsonStringEnumConverterWithFallback : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => false;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => null;
}
