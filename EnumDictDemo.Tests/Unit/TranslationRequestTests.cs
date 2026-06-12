using System.Reflection;
using EnumDictDemo.Infrastructure;

namespace EnumDictDemo.Tests.Unit;

public class TranslationRequestTests
{
    [Fact]
    public void DefaultValues_AreEmpty()
    {
        var request = new TranslationRequest();

        Assert.Equal(string.Empty, request.DictCode);
        Assert.Equal(string.Empty, request.TargetPropertyName);
        Assert.Null(request.DefaultValue);
        Assert.Null(request.SourceValue);
        Assert.Null(request.TargetProperty);
    }

    [Fact]
    public void TargetProperty_CanBeSet()
    {
        var testObj = new TestDto { Name = "test" };
        var prop = typeof(TestDto).GetProperty(nameof(TestDto.Name))!;

        var request = new TranslationRequest
        {
            TargetObject = testObj,
            DictCode = "code",
            TargetPropertyName = "Name",
            SourceValue = "1",
            TargetProperty = prop
        };

        Assert.Same(prop, request.TargetProperty);
        Assert.Equal("Name", request.TargetProperty.Name);
    }

    [Fact]
    public void AllProperties_Settable()
    {
        var obj = new TestDto();
        var prop = typeof(TestDto).GetProperty(nameof(TestDto.Name));

        var request = new TranslationRequest
        {
            TargetObject = obj,
            SourceValue = "val1",
            DictCode = "sex",
            TargetPropertyName = "SexDesc",
            DefaultValue = "未知",
            TargetProperty = prop
        };

        Assert.Same(obj, request.TargetObject);
        Assert.Equal("val1", request.SourceValue);
        Assert.Equal("sex", request.DictCode);
        Assert.Equal("SexDesc", request.TargetPropertyName);
        Assert.Equal("未知", request.DefaultValue);
        Assert.Same(prop, request.TargetProperty);
    }

    private class TestDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
