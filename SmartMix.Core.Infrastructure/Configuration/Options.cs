namespace SmartMix.Core.Infrastructure.Configuration;

public class PlcOptions
{
    public const string SectionName = "Plc";
    
    public string Host { get; set; } = "192.168.1.100";
    public int Port { get; set; } = 502;
    public byte UnitId { get; set; } = 1;
    public ushort StartAddress { get; set; } = 12288;
    public ushort EndAddress { get; set; } = 16383;
    public ushort BlockSize { get; set; } = 122;
    public int PollIntervalMs { get; set; } = 100;
    public int TimeoutMs { get; set; } = 3000;
    public string RegisterMapCsv { get; set; } = "registers.csv";
    public string RegisterMapType { get; set; } = "Csv"; // Csv or Crevis
}

public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxPoolSize { get; set; } = 100;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableRetry { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
}

public class LoggingOptions
{
    public const string SectionName = "Logging";
    
    public string LogLevel { get; set; } = "Information";
    public string LogPath { get; set; } = "logs";
    public int RetainDays { get; set; } = 30;
    public long MaxFileSizeBytes { get; set; } = 100_000_000;
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = true;
}

public class LineOptions
{
    public const string SectionName = "Line";
    
    public int LineNumber { get; set; } = 1;
    public string Name { get; set; } = "Line 1";
    public int MixerCount { get; set; } = 2;
    public int DoserCount { get; set; } = 4;
    public int SkipCount { get; set; } = 1;
    public int AccumulatorCount { get; set; } = 1;
    public int BunkerCount { get; set; } = 12;
    public bool EnableAutoStart { get; set; } = false;
}

public class LicenseOptions
{
    public const string SectionName = "License";
    
    public string SerialNumber { get; set; } = string.Empty;
    public bool AutoActivate { get; set; } = false;
    public int CheckIntervalHours { get; set; } = 24;
}