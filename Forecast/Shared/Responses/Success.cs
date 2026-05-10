using System.Net;

namespace Forecast.Shared.Responses;

public static class Success
{
    public static Success<T> Create<T>(HttpStatusCode code, string message, T data) =>
        new((ushort)code, data, message);

    public static Success<T> Create<T>(ushort code, string message, T data) =>
        new(code, data, message);
}

public record Success<T>(ushort Code, T Data, string Message);
