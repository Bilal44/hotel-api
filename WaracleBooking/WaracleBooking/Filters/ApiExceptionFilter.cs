using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WaracleBooking.Exceptions;

namespace WaracleBooking.Filters;

public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var controllerName = (string)context.RouteData.Values["controller"]!;
        var actionName = (string)context.RouteData.Values["action"]!;

        context.ExceptionHandled = true;

        logger.LogError(
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