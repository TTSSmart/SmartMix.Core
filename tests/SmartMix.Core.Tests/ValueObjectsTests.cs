using FluentAssertions;
using SmartMix.Core.Domain.ValueObjects;
using Xunit;

namespace SmartMix.Core.Tests.ValueObjects;

public class MaterialWeightTests
{
    [Fact]
    public void FromKg_CreatesCorrectWeight()
    {
        var weight = MaterialWeight.FromKg(100.5m);
        
        weight.Kilograms.Should().Be(100.5m);
        weight.Grams.Should().Be(100500);
        weight.Tons.Should().Be(0.1005m);
    }
    
    [Fact]
    public void FromGrams_CreatesCorrectWeight()
    {
        var weight = MaterialWeight.FromGrams(50000);
        
        weight.Kilograms.Should().Be(50m);
        weight.Grams.Should().Be(50000);
    }
    
    [Fact]
    public void Addition_WorksCorrectly()
    {
        var w1 = MaterialWeight.FromKg(100);
        var w2 = MaterialWeight.FromKg(50.5m);
        
        var result = w1 + w2;
        
        result.Kilograms.Should().Be(150.5m);
    }
    
    [Fact]
    public void Subtraction_WorksCorrectly()
    {
        var w1 = MaterialWeight.FromKg(100);
        var w2 = MaterialWeight.FromKg(30);
        
        var result = w1 - w2;
        
        result.Kilograms.Should().Be(70);
    }
    
    [Fact]
    public void Multiplication_WorksCorrectly()
    {
        var weight = MaterialWeight.FromKg(100);
        
        var result = weight * 1.5m;
        
        result.Kilograms.Should().Be(150);
    }
    
    [Fact]
    public void Division_WorksCorrectly()
    {
        var weight = MaterialWeight.FromKg(100);
        
        var result = weight / 4;
        
        result.Kilograms.Should().Be(25);
    }
    
    [Fact]
    public void ImplicitConversion_ToDecimal_Works()
    {
        MaterialWeight weight = MaterialWeight.FromKg(75.5m);
        
        decimal kg = weight;
        
        kg.Should().Be(75.5m);
    }
    
    [Fact]
    public void ExplicitConversion_FromDecimal_Works()
    {
        var weight = (MaterialWeight)123.456m;
        
        weight.Kilograms.Should().Be(123.456m);
    }
    
    [Fact]
    public void Zero_IsZero()
    {
        MaterialWeight.Zero.IsZero.Should().BeTrue();
        MaterialWeight.FromKg(0).IsZero.Should().BeTrue();
        MaterialWeight.FromKg(0.001m).IsZero.Should().BeFalse();
    }
    
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var weight = MaterialWeight.FromKg(123.456m);
        
        weight.ToString().Should().Be("123.456 кг");
    }
}

public class MoistureTests
{
    [Fact]
    public void FromPercent_ClampsToValidRange()
    {
        Moisture.FromPercent(-10).Percent.Should().Be(0);
        Moisture.FromPercent(150).Percent.Should().Be(100);
        Moisture.FromPercent(50.5m).Percent.Should().Be(50.5m);
    }
    
    [Fact]
    public void FromRaw_ConvertsCorrectly()
    {
        var moisture = Moisture.FromRaw(500, 0.1m);
        
        moisture.Percent.Should().Be(50);
    }
    
    [Fact]
    public void Fraction_ReturnsCorrectValue()
    {
        var moisture = Moisture.FromPercent(75);
        
        moisture.Fraction.Should().Be(0.75m);
    }
    
    [Fact]
    public void Addition_WorksCorrectly()
    {
        var m1 = Moisture.FromPercent(30);
        var m2 = Moisture.FromPercent(20);
        
        var result = m1 + m2;
        
        result.Percent.Should().Be(50);
    }
    
    [Fact]
    public void IsValid_ReturnsTrueForValidRange()
    {
        Moisture.FromPercent(0).IsValid.Should().BeTrue();
        Moisture.FromPercent(100).IsValid.Should().BeTrue();
        Moisture.FromPercent(50).IsValid.Should().BeTrue();
    }
}

public class TemperatureTests
{
    [Fact]
    public void FromCelsius_CreatesCorrectTemperature()
    {
        var temp = Temperature.FromCelsius(25.5m);
        
        temp.Celsius.Should().Be(25.5m);
        temp.Fahrenheit.Should().Be(77.9m);
        temp.Kelvin.Should().Be(298.65m);
    }
    
    [Fact]
    public void FromFahrenheit_ConvertsCorrectly()
    {
        var temp = Temperature.FromFahrenheit(77);
        
        temp.Celsius.Should().Be(25m);
    }
    
    [Fact]
    public void FromKelvin_ConvertsCorrectly()
    {
        var temp = Temperature.FromKelvin(298.15m);
        
        temp.Celsius.Should().Be(25m);
    }
    
    [Fact]
    public void Addition_WorksCorrectly()
    {
        var t1 = Temperature.FromCelsius(20);
        var t2 = Temperature.FromCelsius(5);
        
        var result = t1 + t2;
        
        result.Celsius.Should().Be(25);
    }
}

public class VolumeTests
{
    [Fact]
    public void FromCubicMeters_CreatesCorrectVolume()
    {
        var vol = Volume.FromCubicMeters(5.5m);
        
        vol.CubicMeters.Should().Be(5.5m);
        vol.Liters.Should().Be(5500m);
    }
    
    [Fact]
    public void FromLiters_ConvertsCorrectly()
    {
        var vol = Volume.FromLiters(2500);
        
        vol.CubicMeters.Should().Be(2.5m);
    }
    
    [Fact]
    public void Addition_WorksCorrectly()
    {
        var v1 = Volume.FromCubicMeters(2);
        var v2 = Volume.FromCubicMeters(3.5m);
        
        var result = v1 + v2;
        
        result.CubicMeters.Should().Be(5.5m);
    }
}

public class PlcEndpointTests
{
    [Fact]
    public void DefaultPort_Is502()
    {
        var endpoint = new PlcEndpoint("192.168.1.100");
        
        endpoint.Port.Should().Be(502);
        endpoint.UnitId.Should().Be(1);
    }
    
    [Fact]
    public void Validate_ThrowsOnEmptyHost()
    {
        var endpoint = new PlcEndpoint("", 502);
        
        var act = () => endpoint.Validate();
        
        act.Should().Throw<ArgumentException>().WithMessage("*host*");
    }
    
    [Fact]
    public void Validate_ThrowsOnInvalidPort()
    {
        var endpoint = new PlcEndpoint("192.168.1.100", 0);
        
        var act = () => endpoint.Validate();
        
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Port*");
    }
    
    [Fact]
    public void ConnectionString_ReturnsCorrectFormat()
    {
        var endpoint = new PlcEndpoint("192.168.1.100", 502, 1);
        
        endpoint.ConnectionString.Should().Be("192.168.1.100:502");
    }
}