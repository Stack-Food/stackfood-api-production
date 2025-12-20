using FluentAssertions;
using Moq;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Tests.UseCases;

public class GetProductionQueueUseCaseTests
{
    private readonly Mock<IProductionRepository> _repositoryMock;
    private readonly GetProductionQueueUseCase _useCase;

    public GetProductionQueueUseCaseTests()
    {
        _repositoryMock = new Mock<IProductionRepository>();
        _useCase = new GetProductionQueueUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyQueue_WhenNoOrders()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetQueueAsync())
            .ReturnsAsync(new List<ProductionOrder>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.InQueue.Should().BeEmpty();
        result.InProgress.Should().BeEmpty();
        result.Ready.Should().BeEmpty();
        result.TotalInQueue.Should().Be(0);
        result.TotalInProgress.Should().Be(0);
        result.TotalReady.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldOrganizeOrdersByStatus()
    {
        // Arrange
        var receivedOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Received,
            Priority = 1
        };

        var inProgressOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = ProductionStatus.InProgress
        };
        inProgressOrder.StartProduction();

        var readyOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            Status = ProductionStatus.Ready
        };
        readyOrder.StartProduction();
        readyOrder.MarkAsReady();

        var allOrders = new List<ProductionOrder> { receivedOrder, inProgressOrder, readyOrder };

        _repositoryMock
            .Setup(x => x.GetQueueAsync())
            .ReturnsAsync(allOrders);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.InQueue.Should().HaveCount(1);
        result.InQueue[0].OrderNumber.Should().Be("ORD-001");
        result.TotalInQueue.Should().Be(1);

        result.InProgress.Should().HaveCount(1);
        result.InProgress[0].OrderNumber.Should().Be("ORD-002");
        result.TotalInProgress.Should().Be(1);

        result.Ready.Should().HaveCount(1);
        result.Ready[0].OrderNumber.Should().Be("ORD-003");
        result.TotalReady.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSortInQueueByPriorityAndCreatedAt()
    {
        // Arrange
        var order1 = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Received,
            Priority = 2,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var order2 = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = ProductionStatus.Received,
            Priority = 1,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var order3 = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            Status = ProductionStatus.Received,
            Priority = 1,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        var allOrders = new List<ProductionOrder> { order1, order2, order3 };

        _repositoryMock
            .Setup(x => x.GetQueueAsync())
            .ReturnsAsync(allOrders);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.InQueue.Should().HaveCount(3);
        // Priority 1 comes first, then sorted by CreatedAt (oldest first)
        result.InQueue[0].OrderNumber.Should().Be("ORD-003"); // Priority 1, oldest
        result.InQueue[1].OrderNumber.Should().Be("ORD-002"); // Priority 1, newer
        result.InQueue[2].OrderNumber.Should().Be("ORD-001"); // Priority 2
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSortInProgressByStartedAt()
    {
        // Arrange
        var order1 = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-20)
        };

        var order2 = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = ProductionStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var allOrders = new List<ProductionOrder> { order2, order1 };

        _repositoryMock
            .Setup(x => x.GetQueueAsync())
            .ReturnsAsync(allOrders);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.InProgress.Should().HaveCount(2);
        // Sorted by StartedAt (oldest first)
        result.InProgress[0].OrderNumber.Should().Be("ORD-001");
        result.InProgress[1].OrderNumber.Should().Be("ORD-002");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSortReadyByReadyAtDescending()
    {
        // Arrange
        var order1 = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Ready,
            ReadyAt = DateTime.UtcNow.AddMinutes(-20)
        };

        var order2 = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = ProductionStatus.Ready,
            ReadyAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var allOrders = new List<ProductionOrder> { order1, order2 };

        _repositoryMock
            .Setup(x => x.GetQueueAsync())
            .ReturnsAsync(allOrders);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Ready.Should().HaveCount(2);
        // Sorted by ReadyAt (newest first - descending)
        result.Ready[0].OrderNumber.Should().Be("ORD-002");
        result.Ready[1].OrderNumber.Should().Be("ORD-001");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExcludeDeliveredOrders()
    {
        // Arrange
        var receivedOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Received
        };

        var deliveredOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = ProductionStatus.Delivered
        };

        var allOrders = new List<ProductionOrder> { receivedOrder, deliveredOrder };

        _repositoryMock
            .Setup(x => x.GetQueueAsync())
            .ReturnsAsync(allOrders);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.InQueue.Should().HaveCount(1);
        result.InProgress.Should().BeEmpty();
        result.Ready.Should().BeEmpty();
        result.TotalInQueue.Should().Be(1);
        result.TotalInProgress.Should().Be(0);
        result.TotalReady.Should().Be(0);
    }
}
