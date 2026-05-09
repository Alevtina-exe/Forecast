using System.Net;

namespace Forecast.Shared.Responses;

static class Success
{
    public static Success<T> Create<T>(HttpStatusCode code, string message, T data) =>
        new((ushort)code, data, message);

    public static Success<T> Create<T>(ushort code, string message, T data) =>
        new(code, data, message);
}

record Success<T>(ushort Code, T Data, string Message);
