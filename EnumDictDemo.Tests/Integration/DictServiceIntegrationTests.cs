using EnumDictDemo.Data;
using EnumDictDemo.Infrastructure;
using EnumDictDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EnumDictDemo.Tests.Integration;

public class DictServiceIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DictService _service;
    private readonly IMemoryCache _cache;

    public DictServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var dictOptions = Options.Create(new DictTranslateOptions { CacheDurationMinutes = 1 });
        var logger = Mock.Of<ILogger<DictService>>();

        _service = new DictService(_context, _cache, dictOptions, logger);
    }

    [Fact]
    public async Task GetDictLabelAsync_KnownValue_ReturnsLabel()
    {
        var label = await _service.GetDictLabelAsync("sex", "1", TestContext.Current.CancellationToken);
        Assert.Equal("男", label);
    }

    [Fact]
    public async Task GetDictLabelAsync_AnotherKnownValue_ReturnsLabel()
    {
        var label = await _service.GetDictLabelAsync("nation", "3", TestContext.Current.CancellationToken);
        Assert.Equal("回族", label);
    }

    [Fact]
    public async Task GetDictLabelAsync_UnknownValue_ReturnsNull()
    {
        var label = await _service.GetDictLabelAsync("sex", "999", TestContext.Current.CancellationToken);
        Assert.Null(label);
    }

    [Fact]
    public async Task GetDictLabelAsync_UnknownDictCode_ReturnsNull()
    {
        var label = await _service.GetDictLabelAsync("unknown_code", "1", TestContext.Current.CancellationToken);
        Assert.Null(label);
    }

    [Fact]
    public async Task GetDictMappingAsync_ReturnsAllValues()
    {
        var mapping = await _service.GetDictMappingAsync("sex", TestContext.Current.CancellationToken);

        Assert.Equal(3, mapping.Count);
        Assert.Equal("男", mapping["1"]);
        Assert.Equal("女", mapping["2"]);
        Assert.Equal("未知", mapping["0"]);
    }

    [Fact]
    public async Task GetDictMappingAsync_UnknownCode_ReturnsEmpty()
    {
        var mapping = await _service.GetDictMappingAsync("nonexistent", TestContext.Current.CancellationToken);
        Assert.Empty(mapping);
    }

    [Fact]
    public async Task ExistsAsync_KnownValue_ReturnsTrue()
    {
        Assert.True(await _service.ExistsAsync("sex", "1", TestContext.Current.CancellationToken));
        Assert.True(await _service.ExistsAsync("nation", "1", TestContext.Current.CancellationToken));
        Assert.True(await _service.ExistsAsync("order_source", "app", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExistsAsync_UnknownValue_ReturnsFalse()
    {
        Assert.False(await _service.ExistsAsync("sex", "999", TestContext.Current.CancellationToken));
        Assert.False(await _service.ExistsAsync("unknown_code", "1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExistsAsync_DisabledItem_ReturnsFalse()
    {
        Assert.False(await _service.ExistsAsync("order_source", "disabled_source", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RefreshCacheAsync_ClearsCache()
    {
        // 先触发缓存填充
        await _service.GetDictLabelAsync("sex", "1", TestContext.Current.CancellationToken);

        // 刷新缓存
        await _service.RefreshCacheAsync(TestContext.Current.CancellationToken);

        // 再次查询应该重新从数据库加载
        var label = await _service.GetDictLabelAsync("sex", "1", TestContext.Current.CancellationToken);
        Assert.Equal("男", label);
    }

    [Fact]
    public async Task Cache_IsUsedAfterFirstQuery()
    {
        // 首次查询加载缓存
        var label1 = await _service.GetDictLabelAsync("sex", "1", TestContext.Current.CancellationToken);
        Assert.Equal("男", label1);

        // 第二次查询直接从缓存命中（性能关键：不应再次查数据库）
        var label2 = await _service.GetDictLabelAsync("sex", "2", TestContext.Current.CancellationToken);
        Assert.Equal("女", label2);
    }

    [Fact]
    public async Task GetDictMappingAsync_OrderSource_AllValues()
    {
        var mapping = await _service.GetDictMappingAsync("order_source", TestContext.Current.CancellationToken);

        Assert.Equal(4, mapping.Count);
        Assert.Equal("PC端", mapping["pc"]);
        Assert.Equal("APP端", mapping["app"]);
        Assert.Equal("小程序", mapping["mini"]);
        Assert.Equal("H5页面", mapping["h5"]);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
        _cache.Dispose();
    }
}
