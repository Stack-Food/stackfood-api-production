using FluentAssertions;
using Moq;
using StackFood.Production.Application.Configuration;
using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Tests.UseCases;

public class UpdateProductionStatusUseCaseTests
{
    private readonly Mock<IProductionRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProductionSettings _settings;
    private readonly UpdateProductionStatusUseCase _useCase;

    public UpdateProductionStatusUseCaseTests()
    {
        _repositoryMock = new Mock<IProductionRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _settings = new ProductionSettings { SnsTopicArn = "arn:aws:sns:test" };
        _useCase = new UpdateProductionStatusUseCase(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _settings
        );
    }

    [Fact]
    public async Task ExecuteAsync_ToInProgress_ShouldStartProduction()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Received
        };

        var request = new UpdateStatusRequest
        {
            Status = "InProgress"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(order.Id, request);

        // Assert
        result.Status.Should().Be("InProgress");
        order.Status.Should().Be(ProductionStatus.InProgress);
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<object>(), _settings.SnsTopicArn),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ToReady_ShouldMarkAsReady()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-002"
        };
        order.StartProduction();

        var request = new UpdateStatusRequest
        {
            Status = "Ready"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(order.Id, request);

        // Assert
        result.Status.Should().Be("Ready");
        order.Status.Should().Be(ProductionStatus.Ready);
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<object>(), _settings.SnsTopicArn),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ToDelivered_ShouldMarkAsDelivered()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-003"
        };
        order.StartProduction();
        order.MarkAsReady();

        var request = new UpdateStatusRequest
        {
            Status = "Delivered"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(order.Id, request);

        // Assert
        result.Status.Should().Be("Delivered");
        order.Status.Should().Be(ProductionStatus.Delivered);
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<object>(), _settings.SnsTopicArn),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStatus_ShouldThrowException()
    {
        // Arrange
        var order = new ProductionOrder();
        var request = new UpdateStatusRequest { Status = "InvalidStatus" };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        // Act
        var act = () => _useCase.ExecuteAsync(order.Id, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invalid status: InvalidStatus");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderNotFound_ShouldThrowException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateStatusRequest { Status = "Ready" };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((ProductionOrder?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Production order {orderId} not found");
    }

    [Fact]
    public async Task ExecuteAsync_WithEstimatedTime_ShouldSetEstimatedTime()
    {
        // Arrange
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-004"
        };

        var request = new UpdateStatusRequest
        {
            Status = "InProgress",
            EstimatedTime = 20
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);

        // Act
        var result = await _useCase.ExecuteAsync(order.Id, request);

        // Assert
        result.EstimatedTime.Should().Be(20);
    }
}
