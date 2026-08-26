namespace SmartMix.Core.Domain.ValueObjects;

/// <summary>
/// Конечная точка подключения к ПЛК
/// </summary>
public readonly record struct PlcEndpoint
{
    public string Host { get; init; }
    public int Port { get; init; }
    public byte UnitId { get; init; }

    public PlcEndpoint(string host, int port = 502, byte unitId = 1)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("PLC host is required", nameof(host));
        if (port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be 1-65535");

        Host = host;
        Port = port;
        UnitId = unitId;
    }

    public string ConnectionString => $"{Host}:{Port}";
    public override string ToString() => $"{Host}:{Port} (UnitId={UnitId})";
}