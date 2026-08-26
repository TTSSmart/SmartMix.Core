using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Infrastructure.Plc.Variables;

namespace SmartMix.Core.Infrastructure.Plc;

/// <summary>
/// Интерфейс парсера карты регистров
/// </summary>
public interface IPlcRegisterMapParser
{
    Task<RegisterMap> ParseAsync(string csvPath, CancellationToken ct = default);
}

/// <summary>
/// Результат парсинга карты регистров
/// </summary>
public record RegisterMap
{
    public Dictionary<string, RegisterInfo> Registers { get; init; } = new();
    public ushort StartAddress { get; init; }
    public ushort EndAddress { get; init; }
    public ushort FirstNciAddress { get; init; }
}

public record RegisterInfo
{
    public string Name { get; init; } = string.Empty;
    public ushort Address { get; init; }
    public VariableType Type { get; init; }
    public VariableAccessLevel AccessLevel { get; init; }
    public string Description { get; init; } = string.Empty;
    public byte ArraySize { get; init; }
    public byte BitMask { get; init; }
}

/// <summary>
/// Парсер CSV карты регистров
/// </summary>
public class CsvRegisterMapParser : IPlcRegisterMapParser
{
    public async Task<RegisterMap> ParseAsync(string csvPath, CancellationToken ct = default)
    {
        var registers = new Dictionary<string, RegisterInfo>();
        ushort startAddr = 0;
        ushort endAddr = 0;
        ushort firstNciAddr = 0;
        bool needFindNci = true;
        
        var lines = await File.ReadAllLinesAsync(csvPath, ct);
        
        foreach (var line in lines)
        {
            ct.ThrowIfCancellationRequested();
            
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";;") || line.StartsWith("#"))
                continue;
            
            try
            {
                var parts = line.Split(';');
                if (parts.Length < 4) continue;
                
                // Тип: WORD:1 или WORD
                var typePart = parts[0].Trim();
                VariableType type = VariableType.Int;
                byte arraySize = 1;
                
                if (typePart.Contains(':'))
                {
                    var typeParts = typePart.Split(':');
                    type = ParseVariableType(typeParts[0]);
                    arraySize = byte.Parse(typeParts[1]);
                }
                else
                {
                    type = ParseVariableType(typePart);
                }
                
                var name = parts[1].Trim();
                if (string.IsNullOrEmpty(name)) continue;
                
                // Адрес
                ushort address = 0;
                byte bitMask = 0;
                
                var addrPart = parts[2].Trim();
                if (addrPart.Contains('.'))
                {
                    var addrParts = addrPart.Split('.');
                    address = ushort.Parse(addrParts[0]);
                    bitMask = byte.Parse(addrParts[1]);
                }
                else
                {
                    address = ushort.Parse(addrPart);
                }
                
                // Access level
                var accessLevel = ParseAccessLevel(parts[3].Trim());
                
                // Description
                var description = parts.Length > 4 ? parts[4].Trim() : string.Empty;
                
                // Обновляем границы
                if (address > endAddr)
                {
                    endAddr = address;
                    if (type == VariableType.Float) endAddr++;
                }
                
                if (needFindNci && name.Contains("nci", StringComparison.OrdinalIgnoreCase))
                {
                    firstNciAddr = address;
                    needFindNci = false;
                }
                
                registers[name] = new RegisterInfo
                {
                    Name = name,
                    Address = address,
                    Type = type,
                    AccessLevel = accessLevel,
                    Description = description,
                    ArraySize = arraySize,
                    BitMask = bitMask
                };
            }
            catch (Exception ex)
            {
                // Логировать ошибку парсинга строки
            }
        }
        
        return new RegisterMap
        {
            Registers = registers,
            StartAddress = startAddr,
            EndAddress = endAddr,
            FirstNciAddress = firstNciAddr
        };
    }
    
    private VariableType ParseVariableType(string typeStr)
    {
        return typeStr.ToUpper() switch
        {
            "BOOL" or "BIT" => VariableType.Bool,
            "INT" or "WORD" => VariableType.Int,
            "UINT" or "DWORD" => VariableType.Uint,
            "FLOAT" or "REAL" => VariableType.Float,
            "ARRAY" => VariableType.Array,
            _ => VariableType.Int
        };
    }
    
    private VariableAccessLevel ParseAccessLevel(string accessStr)
    {
        return accessStr.ToUpper() switch
        {
            "R" or "READ" => VariableAccessLevel.Read,
            "RW" or "READWRITE" => VariableAccessLevel.ReadWrite,
            _ => VariableAccessLevel.Read
        };
    }
}

/// <summary>
/// Парсер Crevis (для совместимости)
/// </summary>
public class CrevisRegisterMapParser : IPlcRegisterMapParser
{
    public async Task<RegisterMap> ParseAsync(string csvPath, CancellationToken ct = default)
    {
        // Формат Crevis может отличаться - реализовать при необходимости
        var csvParser = new CsvRegisterMapParser();
        return await csvParser.ParseAsync(csvPath, ct);
    }
}