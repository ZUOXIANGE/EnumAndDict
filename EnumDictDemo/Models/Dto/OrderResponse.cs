using EnumDictDemo.Attributes;
using EnumDictDemo.Models.Enums;

namespace EnumDictDemo.Models.Dto;

public class OrderResponse
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public OrderStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodLabel { get; set; } = string.Empty;

    [DictTranslate("order_source", nameof(SourceDesc))]
    public string Source { get; set; } = string.Empty;

    public string SourceDesc { get; set; } = string.Empty;

    public UserResponse? Buyer { get; set; }

    public void FillEnumLabels()
    {
        StatusLabel = Status.GetDisplayName();
        PaymentMethodLabel = PaymentMethod.GetDisplayName();
    }
}