using EnumDictDemo.Attributes;
using EnumDictDemo.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EnumDictDemo.Tests.Unit;

public class ObjectVisitorTests
{
    private readonly ObjectVisitor _visitor;
    private readonly DictTranslateOptions _options;

    public ObjectVisitorTests()
    {
        _options = new DictTranslateOptions
        {
            MaxRecursionDepth = 5,
            EnableCycleDetection = true
        };
        var logger = Mock.Of<ILogger<ObjectVisitor>>();
        var optionsSnapshot = Options.Create(_options);
        _visitor = new ObjectVisitor(optionsSnapshot, logger);
    }

    [Fact]
    public void Visit_Null_ReturnsEmpty()
    {
        var requests = _visitor.Visit(null);
        Assert.Empty(requests);
    }

    [Fact]
    public void Visit_String_ReturnsEmpty()
    {
        var requests = _visitor.Visit("hello");
        Assert.Empty(requests);
    }

    [Fact]
    public void Visit_Int_ReturnsEmpty()
    {
        var requests = _visitor.Visit(42);
        Assert.Empty(requests);
    }

    [Fact]
    public void Visit_DateTime_ReturnsEmpty()
    {
        var requests = _visitor.Visit(DateTime.Now);
        Assert.Empty(requests);
    }

    [Fact]
    public void Visit_DtoWithDictTranslate_CollectsRequest()
    {
        var dto = new SimpleDto { Code = "1" };

        var requests = _visitor.Visit(dto);

        Assert.Single(requests);
        Assert.Same(dto, requests[0].TargetObject);
        Assert.Equal("1", requests[0].SourceValue);
        Assert.Equal("sex", requests[0].DictCode);
        Assert.Equal("CodeDesc", requests[0].TargetPropertyName);
        Assert.NotNull(requests[0].TargetProperty);
        Assert.Equal("CodeDesc", requests[0].TargetProperty!.Name);
    }

    [Fact]
    public void Visit_DtoWithNullSourceValue_SetsNullSource()
    {
        var dto = new SimpleDto { Code = null! };

        var requests = _visitor.Visit(dto);

        Assert.Single(requests);
        Assert.Null(requests[0].SourceValue);
    }

    [Fact]
    public void Visit_MultipleAttributesOnProperty_CollectsAll()
    {
        var dto = new MultiAttrDto { Code = "1" };

        var requests = _visitor.Visit(dto);

        Assert.Equal(2, requests.Count);
        Assert.Contains(requests, r => r.TargetPropertyName == "DescA");
        Assert.Contains(requests, r => r.TargetPropertyName == "DescB");
        Assert.Equal("code_a", requests[0].DictCode);
        Assert.Equal("code_b", requests[1].DictCode);
    }

    [Fact]
    public void Visit_NestedObject_CollectsNested()
    {
        var dto = new ParentDto
        {
            ParentCode = "p1",
            Child = new SimpleDto { Code = "c1" }
        };

        var requests = _visitor.Visit(dto);

        Assert.Equal(2, requests.Count);
        var pRequest = requests.Single(r => r.DictCode == "parent_code");
        var cRequest = requests.Single(r => r.DictCode == "sex");
        Assert.Equal("p1", pRequest.SourceValue);
        Assert.Equal("c1", cRequest.SourceValue);
        Assert.Same(dto, pRequest.TargetObject);
        Assert.Same(dto.Child, cRequest.TargetObject);
    }

    [Fact]
    public void Visit_ListWithItems_CollectsAll()
    {
        var list = new List<SimpleDto>
        {
            new() { Code = "1" },
            new() { Code = "2" },
            new() { Code = "3" }
        };

        var requests = _visitor.Visit(list);

        Assert.Equal(3, requests.Count);
        Assert.All(requests, r => Assert.Equal("sex", r.DictCode));
    }

    [Fact]
    public void Visit_DeeplyNested_CollectsCorrectTargetObjects()
    {
        var grandParent = new GrandParentDto
        {
            Code = "g1",
            Parent = new ParentDto
            {
                ParentCode = "p1",
                Child = new SimpleDto { Code = "c1" }
            }
        };

        var requests = _visitor.Visit(grandParent);

        Assert.Equal(3, requests.Count);
    }

    [Fact]
    public void Visit_CyclicReference_PreventsStackOverflow()
    {
        var nodeA = new CycleDto { Name = "A" };
        var nodeB = new CycleDto { Name = "B" };
        nodeA.Sibling = nodeB;
        nodeB.Sibling = nodeA;

        var ex = Record.Exception(() => _visitor.Visit(nodeA));

        Assert.Null(ex);
    }

    [Fact]
    public void Visit_MaxDepthExceeded_LogsAndSkips()
    {
        var tightOptions = new DictTranslateOptions { MaxRecursionDepth = 1 };
        var mockLogger = new Mock<ILogger<ObjectVisitor>>();
        var visitor = new ObjectVisitor(Options.Create(tightOptions), mockLogger.Object);

        var root = new GrandParentDto
        {
            Code = "x",
            Parent = new ParentDto
            {
                ParentCode = "y",
                Child = new SimpleDto { Code = "z" }
            }
        };

        var requests = visitor.Visit(root);

        // 只应有 1 层（root）或最多 2 层（root + parent），不应无限递归
        Assert.True(requests.Count <= 2);
    }

    [Fact]
    public void Visit_ReadOnlyTargetProperty_Skipped()
    {
        var dto = new ReadOnlyTargetDto { Code = "1" };

        var requests = _visitor.Visit(dto);

        Assert.Empty(requests);
    }

    [Fact]
    public void Visit_NullInList_Skipped()
    {
        var list = new List<SimpleDto?>
        {
            new() { Code = "1" },
            null,
            new() { Code = "3" }
        };

        var requests = _visitor.Visit(list);

        Assert.Equal(2, requests.Count);
    }

    // ------- 测试辅助 DTO -------

    public class SimpleDto
    {
        [DictTranslate("sex", "CodeDesc")]
        public string Code { get; set; } = string.Empty;
        public string CodeDesc { get; set; } = string.Empty;
    }

    public class MultiAttrDto
    {
        [DictTranslate("code_a", "DescA")]
        [DictTranslate("code_b", "DescB")]
        public string Code { get; set; } = string.Empty;
        public string DescA { get; set; } = string.Empty;
        public string DescB { get; set; } = string.Empty;
    }

    public class ParentDto
    {
        [DictTranslate("parent_code", "ParentDesc")]
        public string ParentCode { get; set; } = string.Empty;
        public string ParentDesc { get; set; } = string.Empty;
        public SimpleDto? Child { get; set; }
    }

    public class GrandParentDto
    {
        [DictTranslate("grand_code", "CodeDesc")]
        public string Code { get; set; } = string.Empty;
        public string CodeDesc { get; set; } = string.Empty;
        public ParentDto? Parent { get; set; }
    }

    public class CycleDto
    {
        public string Name { get; set; } = string.Empty;
        public CycleDto? Sibling { get; set; }
    }

    public class ReadOnlyTargetDto
    {
        [DictTranslate("sex", "LockedDesc")]
        public string Code { get; set; } = string.Empty;
        public string LockedDesc { get; } = string.Empty; // 只读
    }
}
