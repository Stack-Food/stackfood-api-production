using FluentAssertions;
using Moq;
using StackFood.Production.Application.Configuration;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Tests.UseCases;

public class StartProductionUseCaseTests
{
    private readonly Mock<IProductionRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProductionSettings _settings;
    private readonly StartProductionUseCase _useCase;

    public StartProductionUseCaseTests()
    {
        _repositoryMock = new Mock<IProductionRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _settings = new ProductionSettings { SnsTopicArn = "arn:aws:sns:test" };
        _useCase = new StartProductionUseCase(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _settings
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStartProduction()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Received
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("InProgress");
        order.Status.Should().Be(ProductionStatus.InProgress);
        order.StartedAt.Should().NotBeNull();

        _repositoryMock.Verify(x => x.GetByIdAsync(order.Id), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(order), Times.Once);
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<object>(), _settings.SnsTopicArn),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithEstimatedTime_ShouldSetEstimatedTime()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = ProductionStatus.Received
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(order.Id, estimatedTime: 15);

        // Assert
        result.EstimatedTime.Should().Be(15);
        order.EstimatedTime.Should().Be(15);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderNotFound_ShouldThrowException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((ProductionOrder?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Production order {orderId} not found");
    }
}
