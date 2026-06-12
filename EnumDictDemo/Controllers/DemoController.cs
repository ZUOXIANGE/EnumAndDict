using EnumDictDemo.Models.Dto;
using EnumDictDemo.Models.Enums;
using EnumDictDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnumDictDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly IDictService _dictService;

    public DemoController(IDictService dictService)
    {
        _dictService = dictService;
    }

    [HttpGet("user")]
    public IActionResult GetUser()
    {
        var user = new UserResponse
        {
            Id = 1,
            Name = "张三",
            Sex = "1",
            Nation = "3",
            Age = 28,
            Email = "zhangsan@example.com"
        };

        return Ok(new { code = 200, data = user });
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        var users = new List<UserResponse>
        {
            new() { Id = 1, Name = "张三", Sex = "1", Nation = "3", Age = 28, Email = "zhangsan@example.com" },
            new() { Id = 2, Name = "李四", Sex = "2", Nation = "1", Age = 25, Email = "lisi@example.com" },
            new() { Id = 3, Name = "王五", Sex = "1", Nation = "4", Age = 32, Email = "wangwu@example.com" }
        };

        return Ok(new { code = 200, data = users });
    }

    [HttpGet("order")]
    public IActionResult GetOrder()
    {
        var order = new OrderResponse
        {
            Id = 1001,
            OrderNo = "ORD-2026-001",
            Amount = 299.99m,
            Status = OrderStatus.Paid,
            PaymentMethod = PaymentMethod.WeChatPay,
            Source = "app",
            Buyer = new UserResponse
            {
                Id = 1,
                Name = "张三",
                Sex = "1",
                Nation = "3",
                Age = 28,
                Email = "zhangsan@example.com"
            }
        };

        return Ok(new { code = 200, data = order });
    }

    [HttpGet("orders")]
    public IActionResult GetOrders()
    {
        var orders = new List<OrderResponse>
        {
            new()
            {
                Id = 1001, OrderNo = "ORD-2026-001", Amount = 299.99m,
                Status = OrderStatus.Paid, PaymentMethod = PaymentMethod.WeChatPay,
                Source = "app",
                Buyer = new UserResponse { Id = 1, Name = "张三", Sex = "1", Nation = "3", Age = 28, Email = "zhangsan@example.com" }
            },
            new()
            {
                Id = 1002, OrderNo = "ORD-2026-002", Amount = 1599.00m,
                Status = OrderStatus.Shipped, PaymentMethod = PaymentMethod.Alipay,
                Source = "pc",
                Buyer = new UserResponse { Id = 2, Name = "李四", Sex = "2", Nation = "1", Age = 25, Email = "lisi@example.com" }
            },
            new()
            {
                Id = 1003, OrderNo = "ORD-2026-003", Amount = 49.90m,
                Status = OrderStatus.Pending, PaymentMethod = PaymentMethod.CashOnDelivery,
                Source = "mini",
                Buyer = null
            }
        };

        return Ok(new { code = 200, data = orders });
    }

    [HttpPost("validate-dict-value")]
    public async Task<IActionResult> ValidateDictValue([FromBody] ValidateDictValueRequest request)
    {
        var exists = await _dictService.ExistsAsync(request.DictCode, request.DictValue);
        if (!exists)
            return BadRequest(new { code = 400, message = $"Value '{request.DictValue}' is not valid for dict '{request.DictCode}'" });

        return Ok(new { code = 200, message = "Valid" });
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { code = 400, message = "Invalid request", errors = ModelState });

        var sourceExists = await _dictService.ExistsAsync("order_source", request.Source);
        if (!sourceExists)
            return BadRequest(new { code = 400, message = $"Invalid order source: '{request.Source}'" });

        var order = new OrderResponse
        {
            Id = new Random().Next(2000, 9999),
            OrderNo = request.OrderNo,
            Amount = request.Amount,
            Status = request.Status,
            PaymentMethod = request.PaymentMethod,
            Source = request.Source
        };

        return Ok(new { code = 201, data = order });
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { code = 400, message = "Invalid request", errors = ModelState });

        var sexExists = await _dictService.ExistsAsync("sex", request.Sex);
        if (!sexExists)
            return BadRequest(new { code = 400, message = $"Invalid sex value: '{request.Sex}'" });

        var nationExists = await _dictService.ExistsAsync("nation", request.Nation);
        if (!nationExists)
            return BadRequest(new { code = 400, message = $"Invalid nation value: '{request.Nation}'" });

        var user = new UserResponse
        {
            Id = new Random().Next(100, 999),
            Name = request.Name,
            Sex = request.Sex,
            Nation = request.Nation,
            Age = request.Age,
            Email = request.Email
        };

        return Ok(new { code = 201, data = user });
    }

    [HttpPost("validate-enum")]
    public IActionResult ValidateEnum([FromBody] ValidateEnumRequest request, [FromServices] IEnumInfoService enumInfoService)
    {
        var options = enumInfoService.GetEnumOptions(request.EnumName);
        if (options.Count == 0)
            return BadRequest(new { code = 400, message = $"Enum '{request.EnumName}' not found" });

        var match = options.FirstOrDefault(o =>
            o.Name.Equals(request.EnumValue, StringComparison.OrdinalIgnoreCase) ||
            o.Value.ToString() == request.EnumValue);

        if (match != null)
            return Ok(new { code = 200, message = "Valid", parsedValue = match.Name });

        var validValues = string.Join(", ", options.Select(o => o.Name));
        return BadRequest(new { code = 400, message = $"Invalid value '{request.EnumValue}' for enum '{request.EnumName}'. Valid values: {validValues}" });
    }

    [HttpPost("refresh-cache")]
    public async Task<IActionResult> RefreshCache()
    {
        await _dictService.RefreshCacheAsync();
        return Ok(new { code = 200, message = "Cache refreshed" });
    }
}