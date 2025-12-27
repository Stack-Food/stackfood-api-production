using FluentAssertions;
using Moq;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Tests.UseCases;

public class GetProductionOrderUseCaseTests
{
    private readonly Mock<IProductionRepository> _repositoryMock;
    private readonly GetProductionOrderUseCase _useCase;

    public GetProductionOrderUseCaseTests()
    {
        _repositoryMock = new Mock<IProductionRepository>();
        _useCase = new GetProductionOrderUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Received,
            Priority = 1
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.OrderId.Should().Be(order.OrderId);
        result.OrderNumber.Should().Be(order.OrderNumber);
        result.Status.Should().Be("Received");
        result.Priority.Should().Be(1);

        _repositoryMock.Verify(x => x.GetByIdAsync(order.Id), Times.Once);
    }

    [Fact]
    public async Task ExecuteByIdAsync_ShouldReturnNull_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((ProductionOrder?)null);

        // Act
        var result = await _useCase.ExecuteByIdAsync(orderId);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByOrderIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            OrderNumber = "ORD-002",
            Status = ProductionStatus.InProgress,
            EstimatedTime = 20
        };
        order.StartProduction();

        _repositoryMock
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.OrderId.Should().Be(orderId);
        result.OrderNumber.Should().Be(order.OrderNumber);
        result.Status.Should().Be("InProgress");
        result.EstimatedTime.Should().Be(20);
        result.StartedAt.Should().NotBeNull();

        _repositoryMock.Verify(x => x.GetByOrderIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByOrderIdAsync_ShouldReturnNull_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync((ProductionOrder?)null);

        // Act
        var result = await _useCase.ExecuteByOrderIdAsync(orderId);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByOrderIdAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task ExecuteByIdAsync_ShouldMapItemsCorrectly()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            Status = ProductionStatus.Ready
        };

        var items = new List<ProductionItem>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Pizza Margherita", ProductCategory = "Pizza", Quantity = 2 },
            new() { ProductId = Guid.NewGuid(), ProductName = "Coca Cola", ProductCategory = "Bebida", Quantity = 1 }
        };

        order.SetItems(items);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items[0].ProductName.Should().Be("Pizza Margherita");
        result.Items[0].Quantity.Should().Be(2);
        result.Items[1].ProductName.Should().Be("Coca Cola");
        result.Items[1].Quantity.Should().Be(1);
    }
}
