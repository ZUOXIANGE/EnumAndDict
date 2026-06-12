using EnumDictDemo.Models.Dto;
using EnumDictDemo.Models.Enums;
using EnumDictDemo.Services;
using FastEnumUtility;

namespace EnumDictDemo.Tests.Unit;

public class EnumInfoServiceTests
{
    private readonly EnumInfoService _service;

    public EnumInfoServiceTests()
    {
        _service = new EnumInfoService();
    }

    [Fact]
    public void GetAllEnums_ReturnsAllRegisteredEnums()
    {
        var all = _service.GetAllEnums();

        Assert.True(all.Count >= 2);
        Assert.Contains(nameof(OrderStatus), all.Keys);
        Assert.Contains(nameof(PaymentMethod), all.Keys);
    }

    [Fact]
    public void GetEnumOptions_OrderStatus_ReturnsAllValues()
    {
        var options = _service.GetEnumOptions(nameof(OrderStatus));

        Assert.Equal(6, options.Count);
        Assert.Contains(options, o => o.Name == "Pending" && o.Label == "待支付");
        Assert.Contains(options, o => o.Name == "Paid" && o.Label == "已支付");
        Assert.Contains(options, o => o.Name == "Shipped" && o.Label == "已发货");
        Assert.Contains(options, o => o.Name == "Completed" && o.Label == "已完成");
        Assert.Contains(options, o => o.Name == "Cancelled" && o.Label == "已取消");
        Assert.Contains(options, o => o.Name == "Refunded" && o.Label == "已退款");
    }

    [Fact]
    public void GetEnumOptions_PaymentMethod_ReturnsAllValues()
    {
        var options = _service.GetEnumOptions(nameof(PaymentMethod));

        Assert.Equal(4, options.Count);
        Assert.Contains(options, o => o.Name == "WeChatPay" && o.Label == "微信支付");
        Assert.Contains(options, o => o.Name == "Alipay" && o.Label == "支付宝");
        Assert.Contains(options, o => o.Name == "BankCard" && o.Label == "银行卡");
        Assert.Contains(options, o => o.Name == "CashOnDelivery" && o.Label == "货到付款");
    }

    [Fact]
    public void GetEnumOptions_InvalidName_ReturnsEmpty()
    {
        var options = _service.GetEnumOptions("NonExistentEnum");

        Assert.Empty(options);
    }

    [Fact]
    public void GetEnumOptions_CaseInsensitive()
    {
        var optionsLower = _service.GetEnumOptions("orderstatus");
        var optionsUpper = _service.GetEnumOptions("ORDERSTATUS");

        Assert.NotEmpty(optionsLower);
        Assert.NotEmpty(optionsUpper);
        Assert.Equal(optionsLower.Count, optionsUpper.Count);
    }

    [Fact]
    public void EnumOptionResponse_HasCorrectStructure()
    {
        var options = _service.GetEnumOptions(nameof(OrderStatus));
        var pending = options.Single(o => o.Name == "Pending");

        Assert.Equal(0, pending.Value);
        Assert.Equal("Pending", pending.Name);
        Assert.Equal("待支付", pending.Label);
    }

    [Fact]
    public void GetAllEnums_ReturnsReadableData()
    {
        var all = _service.GetAllEnums();

        foreach (var (key, options) in all)
        {
            Assert.NotEmpty(options);
            foreach (var opt in options)
            {
                Assert.False(string.IsNullOrEmpty(opt.Name));
                Assert.False(string.IsNullOrEmpty(opt.Label));
            }
        }
    }
}
