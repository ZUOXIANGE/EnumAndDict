using EnumDictDemo.Attributes;

namespace EnumDictDemo.Tests.Unit;

public class DictTranslateAttributeTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var attr = new DictTranslateAttribute("sex", "SexDesc", "未知");

        Assert.Equal("sex", attr.DictCode);
        Assert.Equal("SexDesc", attr.TargetProperty);
        Assert.Equal("未知", attr.DefaultValue);
    }

    [Fact]
    public void Constructor_DefaultValueNullByDefault()
    {
        var attr = new DictTranslateAttribute("sex", "SexDesc");

        Assert.Null(attr.DefaultValue);
    }

    [Fact]
    public void AllowMultiple_ReturnsTrue()
    {
        var usage = typeof(DictTranslateAttribute).GetCustomAttributesData()
            .Single(c => c.AttributeType == typeof(AttributeUsageAttribute));

        var allowMultiple = (bool)usage.NamedArguments.Single(a => a.MemberName == "AllowMultiple").TypedValue.Value!;

        Assert.True(allowMultiple);
    }

    [Fact]
    public void AttributeUsage_HasCorrectTarget()
    {
        var targets = typeof(DictTranslateAttribute).GetCustomAttributesData()
            .Single(c => c.AttributeType == typeof(AttributeUsageAttribute));

        var validOn = (AttributeTargets)targets.ConstructorArguments[0].Value!;

        Assert.Equal(AttributeTargets.Property, validOn);
    }
}
