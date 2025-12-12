using FluentAssertions;
using Moq;
using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Domain.Entities;

namespace StackFood.Production.Tests.UseCases;

public class CreateProductionOrderUseCaseTests
{
    private readonly Mock<IProductionRepository> _repositoryMock;
    private readonly CreateProductionOrderUseCase _useCase;

    public CreateProductionOrderUseCaseTests()
    {
        _repositoryMock = new Mock<IProductionRepository>();
        _useCase = new CreateProductionOrderUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateProductionOrder()
    {
        // Arrange
        var request = new CreateProductionOrderRequest
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Priority = 1,
            EstimatedTime = 10,
            Items = new List<ProductionItemDTO>
            {
                new ProductionItemDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "X-Burger",
                    ProductCategory = "Lanche",
                    Quantity = 2
                }
            }
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(request.OrderId);
        result.OrderNumber.Should().Be("ORD-001");
        result.Priority.Should().Be(1);
        result.EstimatedTime.Should().Be(10);
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductName.Should().Be("X-Burger");

        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<ProductionOrder>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapMultipleItems()
    {
        // Arrange
        var request = new CreateProductionOrderRequest
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Priority = 2,
            Items = new List<ProductionItemDTO>
            {
                new ProductionItemDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Burger",
                    ProductCategory = "Lanche",
                    Quantity = 1
                },
                new ProductionItemDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Fries",
                    ProductCategory = "Acompanhamento",
                    Quantity = 2
                },
                new ProductionItemDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Coke",
                    ProductCategory = "Bebida",
                    Quantity = 1
                }
            }
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].ProductName.Should().Be("Burger");
        result.Items[1].ProductName.Should().Be("Fries");
        result.Items[2].ProductName.Should().Be("Coke");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetItemsWithPreparationNotes()
    {
        // Arrange
        var request = new CreateProductionOrderRequest
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            Items = new List<ProductionItemDTO>
            {
                new ProductionItemDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Custom Burger",
                    ProductCategory = "Lanche",
                    Quantity = 1,
                    PreparationNotes = "No onions, extra cheese"
                }
            }
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Items.First().PreparationNotes.Should().Be("No onions, extra cheese");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnDTOWithCorrectStatus()
    {
        // Arrange
        var request = new CreateProductionOrderRequest
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-004",
            Items = new List<ProductionItemDTO>
            {
                new ProductionItemDTO
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    ProductCategory = "Test",
                    Quantity = 1
                }
            }
        };

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Status.Should().Be("Received");
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
