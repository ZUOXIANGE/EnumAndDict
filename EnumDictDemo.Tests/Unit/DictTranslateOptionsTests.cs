using EnumDictDemo.Infrastructure;

namespace EnumDictDemo.Tests.Unit;

public class DictTranslateOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new DictTranslateOptions();

        Assert.Equal(10, options.MaxRecursionDepth);
        Assert.True(options.EnableBatchTranslation);
        Assert.Equal(60, options.CacheDurationMinutes);
        Assert.False(options.ThrowOnMissingTargetProperty);
        Assert.True(options.EnableCycleDetection);
    }
}
