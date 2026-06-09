namespace EnumDictDemo.Models.Enums;

public static class EnumDisplayNames
{
    public static string GetDisplayName(this OrderStatus value) => value switch
    {
        OrderStatus.Pending => "待支付",
        OrderStatus.Paid => "已支付",
        OrderStatus.Shipped => "已发货",
        OrderStatus.Completed => "已完成",
        OrderStatus.Cancelled => "已取消",
        OrderStatus.Refunded => "已退款",
        _ => value.ToString()
    };

    public static string GetDisplayName(this PaymentMethod value) => value switch
    {
        PaymentMethod.WeChatPay => "微信支付",
        PaymentMethod.Alipay => "支付宝",
        PaymentMethod.BankCard => "银行卡",
        PaymentMethod.CashOnDelivery => "货到付款",
        _ => value.ToString()
    };
}