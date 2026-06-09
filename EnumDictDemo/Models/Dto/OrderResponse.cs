using EnumDictDemo.Attributes;
using EnumDictDemo.Models.Enums;
using FastEnumUtility;

namespace EnumDictDemo.Models.Dto;

public class OrderResponse
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public OrderStatus Status { get; set; }
    public string StatusLabel => Status.GetLabel() ?? Status.ToString();

    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodLabel => PaymentMethod.GetLabel() ?? PaymentMethod.ToString();

    [DictTranslate("order_source", nameof(SourceDesc))]
    public string Source { get; set; } = string.Empty;

    public string SourceDesc { get; set; } = string.Empty;

    public UserResponse? Buyer { get; set; }
}