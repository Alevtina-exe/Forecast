namespace Forecawst.Responses;
public record TemperatureResponse(
    ushort Code,
    decimal Temperature,
    string Message
);