using Microsoft.Extensions.Logging;
using WaracleBooking.Exceptions;
using WaracleBooking.Filters;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System.Net;
using Exception = System.Exception;

namespace WaracleBooking.Tests.Filters
{
    public class ApiExceptionFilterTests
    {
        private readonly ApiExceptionFilter _filter;
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly HttpContext _httpContext;
        private readonly IActionResultExecutor<ObjectResult> _actionResultExecutor;

        public ApiExceptionFilterTests()
        {
            _actionResultExecutor = A.Fake<IActionResultExecutor<ObjectResult>>();
            _logger = A.Fake<ILogger<ApiExceptionFilter>>();
            _filter = new ApiExceptionFilter(_logger);
            _httpContext = new DefaultHttpContext();
        }

        [Fact]
        public void OnException_WithGenericApiException_SetsHandledAndReturnsErrorResponse()
        {
            // [Arrange]
            var error = "An error occurred.";
            var exception = new ApiException(
                HttpStatusCode.BadRequest,
                error);

            var context = CreateExceptionContext(exception);

            // [Act]
            _filter.OnException(context);

            // [Assert]
            A.CallTo(() => _actionResultExecutor.ExecuteAsync(
                    A<ActionContext>._,
                    A<ObjectResult>.That.Matches(o =>
                        o.StatusCode == (int)HttpStatusCode.BadRequest &&
                        o.Value == error)))
                .MustHaveHappened();
        }
        
        
        [Fact]
        public void OnException_WithGenericException_SetsHandledAndReturns500ErrorResponse()
        {
            // [Arrange]
            var exception = new Exception();
            var context = CreateExceptionContext(exception);

            // [Act]
            _filter.OnException(context);

            // [Assert]
            context.ExceptionHandled.Should().BeTrue();
            A.CallTo(() => _actionResultExecutor.ExecuteAsync(
                 A<ActionContext>._,
                 A<ObjectResult>.That.Matches(o =>
                     o.StatusCode == (int)HttpStatusCode.InternalServerError &&
                     o.Value.ToString() == exception.Message)))
             .MustHaveHappened();
            
        }

        private ExceptionContext CreateExceptionContext(Exception exception)
        {
            var services = A.Fake<IServiceProvider>();

            A.CallTo(() => services.GetService(typeof(IActionResultExecutor<ObjectResult>)))
                .Returns(_actionResultExecutor);
            
            _httpContext.RequestServices = services;
            
            var actionContext = new ActionContext
            {
                HttpContext = _httpContext,
                RouteData = new RouteData(new RouteValueDictionary
            {
                { "controller", "SpecialController" },
                { "action", "SpecialAction" }
            }),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            };

            return new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = exception
            };
        }
    }
}