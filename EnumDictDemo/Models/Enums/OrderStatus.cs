using FastEnumUtility;

namespace EnumDictDemo.Models.Enums;

public enum OrderStatus
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
    [Label("已退款")]
    Refunded = 5
}