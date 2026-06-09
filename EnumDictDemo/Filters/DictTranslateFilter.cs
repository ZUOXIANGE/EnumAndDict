using EnumDictDemo.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EnumDictDemo.Filters;

public class DictTranslateFilter : IAsyncResultFilter
{
    private readonly ObjectVisitor _visitor;
    private readonly DictTranslationHelper _helper;
    private readonly ILogger<DictTranslateFilter> _logger;

    public DictTranslateFilter(ObjectVisitor visitor, DictTranslationHelper helper, ILogger<DictTranslateFilter> logger)
    {
        _visitor = visitor;
        _helper = helper;
        _logger = logger;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            var resultValue = objectResult.Value;

            var requests = _visitor.Visit(resultValue);

            if (requests.Count > 0)
            {
                _logger.LogDebug("Found {Count} dict translation requests", requests.Count);
                await _helper.TranslateAsync(requests, context.HttpContext.RequestAborted);
            }
        }

        await next();
    }
}