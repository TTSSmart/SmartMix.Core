using FluentAssertions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.Entities.Mechanisms;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Domain.Services;
using SmartMix.Core.Domain.Specifications;
using SmartMix.Core.Domain.Events;
using Moq;

namespace SmartMix.Core.Tests.DomainServices;

public class RecipeCalculatorTests
{
    [Fact]
    public void CalculateMaterialVolumes_ReturnsCorrectVolumes()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Structures = new List<RecipeStructure>
            {
                new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 15, TargetWeight = MaterialWeight.FromKg(300) },
                new() { ComponentId = 2.ToMaterialId(), ComponentName = "Sand", Percentage = 35, TargetWeight = MaterialWeight.FromKg(700) },
                new() { ComponentId = 3.ToMaterialId(), ComponentName = "Gravel", Percentage = 50, TargetWeight = MaterialWeight.FromKg(1000) }
            }
        };
        
        var totalVolume = Volume.FromCubicMeters(2);
        
        var result = RecipeCalculator.CalculateMaterialVolumes(recipe, totalVolume);
        
        result.Should().HaveCount(3);
        result.Sum(m => m.Volume.CubicMeters).Should().BeApproximately(2, 0.01m);
    }
    
    [Fact]
    public void CalculateHumidityCorrection_ReturnsZero_WhenActualEqualsTarget()
    {
        var dryWeight = MaterialWeight.FromKg(1000);
        var targetMoisture = Moisture.FromPercent(5);
        var actualMoisture = Moisture.FromPercent(5);
        
        var correction = RecipeCalculator.CalculateHumidityCorrection(dryWeight, targetMoisture, actualMoisture);
        
        correction.Should().Be(MaterialWeight.Zero);
    }
    
    [Fact]
    public void CalculateHumidityCorrection_ReturnsPositive_WhenActualLessThanTarget()
    {
        var dryWeight = MaterialWeight.FromKg(1000);
        var targetMoisture = Moisture.FromPercent(6);
        var actualMoisture = Moisture.FromPercent(4);
        
        var correction = RecipeCalculator.CalculateHumidityCorrection(dryWeight, targetMoisture, actualMoisture);
        
        correction.Kilograms.Should().BePositive();
    }
    
    [Fact]
    public void CalculateHumidityCorrection_ReturnsZero_WhenActualGreaterThanTarget()
    {
        var dryWeight = MaterialWeight.FromKg(1000);
        var targetMoisture = Moisture.FromPercent(4);
        var actualMoisture = Moisture.FromPercent(6);
        
        var correction = RecipeCalculator.CalculateHumidityCorrection(dryWeight, targetMoisture, actualMoisture);
        
        correction.Should().Be(MaterialWeight.Zero);
    }
    
    [Fact]
    public void CalculateWaterDose_ReturnsCorrectVolume()
    {
        var totalVolume = Volume.FromCubicMeters(2);
        var targetMoisture = Moisture.FromPercent(6);
        var currentMoisture = Moisture.FromPercent(4);
        var totalDryWeight = MaterialWeight.FromKg(2000);
        
        var waterVolume = RecipeCalculator.CalculateWaterDose(totalVolume, targetMoisture, currentMoisture, totalDryWeight);
        
        waterVolume.CubicMeters.Should().BePositive();
    }
    
    [Fact]
    public void ValidateRecipe_ReturnsValid_ForCorrectRecipe()
    {
        var recipe = new Recipe
        {
            Structures = new List<RecipeStructure>
            {
                new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 15, TargetWeight = MaterialWeight.FromKg(300), Correct = 0 },
                new() { ComponentId = 2.ToMaterialId(), ComponentName = "Sand", Percentage = 85, TargetWeight = MaterialWeight.FromKg(1700), Correct = 0 }
            }
        };
        
        var result = RecipeCalculator.ValidateRecipe(recipe);
        
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void ValidateRecipe_ReturnsInvalid_ForEmptyRecipe()
    {
        var recipe = new Recipe { Structures = new List<RecipeStructure>() };
        
        var result = RecipeCalculator.ValidateRecipe(recipe);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Рецепт не содержит компонентов");
    }
    
    [Fact]
    public void ValidateRecipe_ReturnsWarning_ForIncorrectPercentageSum()
    {
        var recipe = new Recipe
        {
            Structures = new List<RecipeStructure>
            {
                new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 20, TargetWeight = MaterialWeight.FromKg(300) },
                new() { ComponentId = 2.ToMaterialId(), ComponentName = "Sand", Percentage = 70, TargetWeight = MaterialWeight.FromKg(1000) }
            }
        };
        
        var result = RecipeCalculator.ValidateRecipe(recipe);
        
        result.IsValid.Should().BeTrue(); // Предупреждение, не ошибка
        result.Warnings.Should().Contain(w => w.Contains("90%"));
    }
}

public class BatchingOrchestratorTests
{
    private readonly Mock<IDomainEventPublisher> _eventPublisherMock;
    private readonly BatchingOrchestrator _orchestrator;
    
    public BatchingOrchestratorTests()
    {
        _eventPublisherMock = new Mock<IDomainEventPublisher>();
        _orchestrator = new BatchingOrchestrator(_eventPublisherMock.Object);
    }
    
    [Fact]
    public void StartBatch_CreatesContext_And_PublishesEvent()
    {
        var application = new Application
        {
            Id = 1,
            Volume = Volume.FromCubicMeters(2),
            Layers = new List<LayerApplication>
            {
                new() { Recipe = new Recipe { Id = 1, Name = "Test Recipe", Structures = new List<RecipeStructure>
                    {
                        new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 100, TargetWeight = MaterialWeight.FromKg(2000) }
                    }}}
            }
        };
        
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Structures = new List<RecipeStructure>
            {
                new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 100, TargetWeight = MaterialWeight.FromKg(2000) }
            }
        };
        
        var result = _orchestrator.StartBatch(application, 1, recipe);
        
        result.Success.Should().BeTrue();
        result.Context.Should().NotBeNull();
        result.Context!.MixerNumber.Should().Be(1);
        result.Context.CurrentStage.Should().Be(BatchingStage.Dosing);
        
        _eventPublisherMock.Verify(p => p.Publish(It.IsAny<BatchStartedEvent>()), Times.Once);
    }
    
    [Fact]
    public void StartBatch_ReturnsFailure_WhenMixerBusy()
    {
        var application = new Application { Id = 1, Volume = Volume.FromCubicMeters(2) };
        var recipe = new Recipe { Id = 1, Structures = new List<RecipeStructure>() };
        
        _orchestrator.StartBatch(application, 1, recipe);
        var result = _orchestrator.StartBatch(application, 1, recipe);
        
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("уже выполняет замес");
    }
    
    [Fact]
    public void ProcessDosingCommand_ReturnsFailure_WhenBatchNotFound()
    {
        var result = _orchestrator.ProcessDosingCommand(999, new SmartMix.Core.Domain.Services.DosingCommand
        {
            MaterialId = 1.ToMaterialId()
        });
        
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Замес не найден");
    }
    
    [Fact]
    public void CompleteBatch_RemovesContext_And_PublishesEvent()
    {
        var application = new Application { Id = 1, Volume = Volume.FromCubicMeters(2) };
        var recipe = new Recipe { Id = 1, Structures = new List<RecipeStructure>
            {
                new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 100, TargetWeight = MaterialWeight.FromKg(2000) }
            }};
        
        _orchestrator.StartBatch(application, 1, recipe);
        
        var result = _orchestrator.CompleteBatch(1, true);
        
        result.Success.Should().BeTrue();
        _orchestrator.GetContext(1).Should().BeNull();
        
        _eventPublisherMock.Verify(p => p.Publish(It.IsAny<BatchCompletedEvent>()), Times.Once);
    }
}

public class ConsumptionTrackerTests
{
    private readonly Mock<IDomainEventPublisher> _eventPublisherMock;
    private readonly ConsumptionTracker _tracker;
    
    public ConsumptionTrackerTests()
    {
        _eventPublisherMock = new Mock<IDomainEventPublisher>();
        _tracker = new ConsumptionTracker(_eventPublisherMock.Object);
    }
    
    [Fact]
    public void RecordConsumption_UpdatesTotals_And_PublishesEvent()
    {
        var materialId = 1.ToMaterialId();
        var weight = MaterialWeight.FromKg(500);
        
        _tracker.RecordConsumption(1, materialId, weight, 100, 1);
        
        var total = _tracker.GetTotalConsumption(materialId);
        var bunker = _tracker.GetBunkerConsumption(1, materialId);
        
        total.Should().Be(weight);
        bunker.Should().Be(weight);
        
        _eventPublisherMock.Verify(p => p.Publish(It.IsAny<MaterialConsumedEvent>()), Times.Once);
    }
    
    [Fact]
    public void RecordConsumption_AccumulatesMultipleCalls()
    {
        var materialId = 1.ToMaterialId();
        
        _tracker.RecordConsumption(1, materialId, MaterialWeight.FromKg(100), 100, 1);
        _tracker.RecordConsumption(1, materialId, MaterialWeight.FromKg(200), 101, 2);
        
        _tracker.GetTotalConsumption(materialId).Kilograms.Should().Be(300);
        _tracker.GetBunkerConsumption(1, materialId).Kilograms.Should().Be(300);
    }
    
    [Fact]
    public void GetReport_ReturnsCorrectData()
    {
        var materialId = 1.ToMaterialId();
        _tracker.RecordConsumption(1, materialId, MaterialWeight.FromKg(100), 100, 1);
        
        var report = _tracker.GetReport(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        
        report.Materials.Should().HaveCount(1);
        report.Materials[0].TotalConsumedKg.Should().Be(100);
    }
    
    [Fact]
    public void ResetCounters_ClearsAllData_And_PublishesEvent()
    {
        _tracker.RecordConsumption(1, 1.ToMaterialId(), MaterialWeight.FromKg(100), 100, 1);
        
        _tracker.ResetCounters();
        
        _tracker.GetTotalConsumption(1.ToMaterialId()).Should().Be(MaterialWeight.Zero);
        _eventPublisherMock.Verify(p => p.Publish(It.IsAny<ConsumptionCountersResetEvent>()), Times.Once);
    }
}

public class SpecificationsTests
{
    [Fact]
    public void CanStartBatch_ReturnsTrue_ForValidApplication()
    {
        var app = new Application
        {
            IsCompleted = false,
            IsDeleted = false,
            IsEditLock = false,
            Volume = Volume.FromCubicMeters(2),
            Layers = new List<LayerApplication> { new() { Recipe = new Recipe() } }
        };
        
        var spec = new BatchingSpecifications.CanStartBatch();
        
        spec.IsSatisfiedBy(app).Should().BeTrue();
    }
    
    [Fact]
    public void CanStartBatch_ReturnsFalse_ForCompletedApplication()
    {
        var app = new Application { IsCompleted = true, Volume = Volume.FromCubicMeters(2), Layers = new List<LayerApplication> { new() } };
        
        var spec = new BatchingSpecifications.CanStartBatch();
        
        spec.IsSatisfiedBy(app).Should().BeFalse();
        spec.GetFailureReason(app).Should().Contain("завершена");
    }
    
    [Fact]
    public void RecipeValidForProduction_ReturnsTrue_ForValidRecipe()
    {
        var recipe = new Recipe
        {
            Structures = new List<RecipeStructure>
            {
                new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 15, TargetWeight = MaterialWeight.FromKg(300) },
                new() { ComponentId = 2.ToMaterialId(), ComponentName = "Sand", Percentage = 85, TargetWeight = MaterialWeight.FromKg(1700) }
            }
        };
        
        var spec = new BatchingSpecifications.RecipeValidForProduction();
        
        spec.IsSatisfiedBy(recipe).Should().BeTrue();
    }
    
    [Fact]
    public void And_CombinesSpecifications()
    {
        var spec1 = new BatchingSpecifications.CanStartBatch();
        var spec2 = new BatchingSpecifications.RecipeValidForProduction();
        
        var combined = spec1.And(spec2);
        
        combined.GetType().Name.Should().Be("AndSpecification`1");
    }
}