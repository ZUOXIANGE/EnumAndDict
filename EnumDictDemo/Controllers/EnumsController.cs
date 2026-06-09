using EnumDictDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnumDictDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnumsController : ControllerBase
{
    private readonly IEnumInfoService _enumInfoService;

    public EnumsController(IEnumInfoService enumInfoService)
    {
        _enumInfoService = enumInfoService;
    }

    [HttpGet]
    public IActionResult GetAllEnums()
    {
        var all = _enumInfoService.GetAllEnums();
        return Ok(new { code = 200, data = all });
    }

    [HttpGet("{enumName}")]
    public IActionResult GetEnum(string enumName)
    {
        var options = _enumInfoService.GetEnumOptions(enumName);
        if (options.Count == 0)
            return NotFound(new { code = 404, message = $"Enum '{enumName}' not found" });

        return Ok(new { code = 200, data = options });
    }
}