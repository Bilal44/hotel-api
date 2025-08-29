using System.Net;

namespace WaracleBooking.Exceptions;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string errorMessage)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public HttpStatusCode StatusCode { get; }
    public string ErrorMessage { get; }
}