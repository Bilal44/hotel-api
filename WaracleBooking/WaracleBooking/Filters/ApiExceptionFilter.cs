using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WaracleBooking.Exceptions;

namespace WaracleBooking.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var controllerName = (string)context.RouteData.Values["controller"]!;
        var actionName = (string)context.RouteData.Values["action"]!;

        context.ExceptionHandled = true;

        _logger.LogError(
            context.Exception,
            "An exception occurred handling the response for Controller [{controllerName}] " +
            "executing Action [{actionName}]",
            controllerName,
            actionName);
        
        if (context.Exception is ApiException ex)
        {
            var actionResult = new ObjectResult(ex.ErrorMessage)
            {
                StatusCode = (int)ex.StatusCode
            };
            actionResult.ExecuteResultAsync(context);
        }
        else
        {
            var actionResult = new ObjectResult(context.Exception.Message)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
            actionResult.ExecuteResultAsync(context);
        }
    }
}