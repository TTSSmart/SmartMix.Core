using FluentAssertions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Application.Recipes;
using SmartMix.Core.Application.Abstractions;
using Moq;
using Xunit;

namespace SmartMix.Core.Tests.Application;

public class RecipeUseCasesTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly Mock<IComponentRepository> _componentRepoMock;
    private readonly Mock<IDomainEventPublisher> _eventPublisherMock;
    private readonly CreateRecipeUseCase _createUseCase;
    private readonly CalculateRecipeMaterialsUseCase _calculateUseCase;
    
    public RecipeUseCasesTests()
    {
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _componentRepoMock = new Mock<IComponentRepository>();
        _eventPublisherMock = new Mock<IDomainEventPublisher>();
        
        _createUseCase = new CreateRecipeUseCase(
            _recipeRepoMock.Object, 
            _componentRepoMock.Object, 
            _eventPublisherMock.Object);
        
        _calculateUseCase = new CalculateRecipeMaterialsUseCase();
    }
    
    [Fact]
    public async Task CreateRecipe_ReturnsFailure_WhenComponentNotFound()
    {
        _componentRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Component?)null);
        
        var command = new CreateRecipeCommand(
            "Test Recipe", true, true, 1, 1, 1, 1,
            new List<SmartMix.Core.Application.Recipes.RecipeStructureDto>
            {
                new(999, "Unknown", 100, 2000, 0)
            });
        
        var result = await _createUseCase.ExecuteAsync(command);
        
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("не найден");
    }
    
    [Fact]
    public async Task CreateRecipe_CreatesRecipe_And_PublishesEvent()
    {
        var component = new Component { Id = 1, Name = "Cement", Type = 1 };
        _componentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(component);
        
        var createdRecipe = new Recipe { Id = 42, Name = "Test Recipe" };
        _recipeRepoMock.Setup(r => r.CreateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);
        
        var command = new CreateRecipeCommand(
            "Test Recipe", true, true, 1, 1, 1, 1,
            new List<SmartMix.Core.Application.Recipes.RecipeStructureDto>
            {
                new(1, "Cement", 100, 2000, 0)
            });
        
        var result = await _createUseCase.ExecuteAsync(command);
        
        result.Success.Should().BeTrue();
        result.Recipe.Should().NotBeNull();
        result.Recipe!.Id.Should().Be(42);
        result.Recipe.Name.Should().Be("Test Recipe");
        
        _recipeRepoMock.Verify(r => r.CreateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(p => p.Publish(It.IsAny<SmartMix.Core.Domain.Events.RecipeCreatedEvent>()), Times.Once);
    }
    
    [Fact]
    public void CalculateMaterials_ReturnsCorrectVolumes()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test",
            Structures = new List<RecipeStructure>
            {
                new() { ComponentId = 1.ToMaterialId(), ComponentName = "Cement", Percentage = 15, TargetWeight = MaterialWeight.FromKg(300), Correct = 0 },
                new() { ComponentId = 2.ToMaterialId(), ComponentName = "Sand", Percentage = 35, TargetWeight = MaterialWeight.FromKg(700), Correct = 0 },
                new() { ComponentId = 3.ToMaterialId(), ComponentName = "Gravel", Percentage = 50, TargetWeight = MaterialWeight.FromKg(1000), Correct = 0 }
            }
        };
        
        var result = _calculateUseCase.Execute(new CalculateMaterialsCommand(recipe, Volume.FromCubicMeters(2)));
        
        result.Success.Should().BeTrue();
        result.Materials.Should().HaveCount(3);
        result.TotalVolume.CubicMeters.Should().Be(2);
    }
}