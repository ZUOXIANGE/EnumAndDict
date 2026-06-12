using EnumDictDemo.Attributes;
using EnumDictDemo.Infrastructure;
using EnumDictDemo.Models.Dto;
using EnumDictDemo.Models.Enums;
using EnumDictDemo.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EnumDictDemo.Tests.Integration;

public class DictTranslateFilterPipelineTests
{
    private readonly Mock<IDictService> _dictServiceMock;
    private readonly ObjectVisitor _visitor;
    private readonly DictTranslationHelper _helper;

    public DictTranslateFilterPipelineTests()
    {
        _dictServiceMock = new Mock<IDictService>();
        SetupDictServiceDefaults();

        var options = Options.Create(new DictTranslateOptions());
        var visitorLogger = Mock.Of<ILogger<ObjectVisitor>>();
        var helperLogger = Mock.Of<ILogger<DictTranslationHelper>>();

        _visitor = new ObjectVisitor(options, visitorLogger);
        _helper = new DictTranslationHelper(_dictServiceMock.Object, helperLogger);
    }

    [Fact]
    public async Task FullPipeline_UserResponse_AllFieldsTranslated()
    {
        var user = new UserResponse
        {
            Id = 1,
            Name = "张三",
            Sex = "1",
            Nation = "3",
            Age = 28,
            Email = "test@example.com"
        };

        var requests = _visitor.Visit(user);
        Assert.Equal(2, requests.Count);

        await _helper.TranslateAsync(requests, TestContext.Current.CancellationToken);

        Assert.Equal("男", user.SexDesc);
        Assert.Equal("回族", user.NationDesc);
    }

    [Fact]
    public async Task FullPipeline_OrderResponse_NestedTranslation()
    {
        var order = new OrderResponse
        {
            Id = 1,
            OrderNo = "ORD-001",
            Amount = 100m,
            Source = "app",
            Status = EnumDictDemo.Models.Enums.OrderStatus.Paid,
            PaymentMethod = EnumDictDemo.Models.Enums.PaymentMethod.WeChatPay,
            Buyer = new UserResponse
            {
                Id = 1, Name = "张三", Sex = "1", Nation = "4", Age = 30
            }
        };

        var requests = _visitor.Visit(order);
        Assert.Equal(3, requests.Count);

        await _helper.TranslateAsync(requests, TestContext.Current.CancellationToken);

        Assert.Equal("APP端", order.SourceDesc);
        Assert.Equal("男", order.Buyer!.SexDesc);
        Assert.Equal("藏族", order.Buyer.NationDesc);
    }

    [Fact]
    public async Task FullPipeline_OrderList_BatchTranslation()
    {
        var orders = new List<OrderResponse>
        {
            new() { Id = 1, OrderNo = "A", Amount = 10, Source = "pc", Status = OrderStatus.Pending, PaymentMethod = PaymentMethod.WeChatPay },
            new() { Id = 2, OrderNo = "B", Amount = 20, Source = "app", Status = OrderStatus.Paid, PaymentMethod = PaymentMethod.Alipay },
            new() { Id = 3, OrderNo = "C", Amount = 30, Source = "mini", Status = OrderStatus.Shipped, PaymentMethod = PaymentMethod.BankCard }
        };

        var requests = _visitor.Visit(orders);
        Assert.Equal(3, requests.Count);

        await _helper.TranslateAsync(requests, TestContext.Current.CancellationToken);

        Assert.Equal("PC端", orders[0].SourceDesc);
        Assert.Equal("APP端", orders[1].SourceDesc);
        Assert.Equal("小程序", orders[2].SourceDesc);
    }

    [Fact]
    public void Visit_ObjectWithoutDictTranslate_ReturnsEmpty()
    {
        var dto = new { Name = "test", Age = 25 };

        var requests = _visitor.Visit(dto);
        Assert.Empty(requests);
    }

    [Fact]
    public void Visit_WrappedResponse_CollectsFromDataProperty()
    {
        var user = new UserResponse { Id = 1, Name = "张三", Sex = "1", Nation = "3" };
        var response = new { code = 200, data = user };

        var requests = _visitor.Visit(response);
        Assert.Equal(2, requests.Count);
        Assert.All(requests, r => Assert.Equal("张三", ((UserResponse)r.TargetObject).Name));
    }

    private void SetupDictServiceDefaults()
    {
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("sex", "1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("男");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("sex", "2", It.IsAny<CancellationToken>()))
            .ReturnsAsync("女");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("sex", "0", It.IsAny<CancellationToken>()))
            .ReturnsAsync("未知");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("nation", "1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("汉族");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("nation", "3", It.IsAny<CancellationToken>()))
            .ReturnsAsync("回族");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("nation", "4", It.IsAny<CancellationToken>()))
            .ReturnsAsync("藏族");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("order_source", "pc", It.IsAny<CancellationToken>()))
            .ReturnsAsync("PC端");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("order_source", "app", It.IsAny<CancellationToken>()))
            .ReturnsAsync("APP端");
        _dictServiceMock
            .Setup(s => s.GetDictLabelAsync("order_source", "mini", It.IsAny<CancellationToken>()))
            .ReturnsAsync("小程序");
    }
}
