using FluentAssertions;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;

namespace StackFood.Production.Tests.Domain;

public class ProductionOrderTests
{
    [Fact]
    public void Constructor_ShouldCreateOrderWithReceivedStatus()
    {
        // Arrange & Act
        var order = new ProductionOrder();

        // Assert
        order.Id.Should().NotBeEmpty();
        order.Status.Should().Be(ProductionStatus.Received);
        order.Priority.Should().Be(1);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.ItemsJson.Should().Be("[]");
    }

    [Fact]
    public void StartProduction_ShouldUpdateStatusAndSetStartedAt()
    {
        // Arrange
        var order = new ProductionOrder
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001"
        };

        // Act
        order.StartProduction();

        // Assert
        order.Status.Should().Be(ProductionStatus.InProgress);
        order.StartedAt.Should().NotBeNull();
        order.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsReady_ShouldUpdateStatusAndSetReadyAt()
    {
        // Arrange
        var order = new ProductionOrder();
        order.StartProduction();

        // Act
        order.MarkAsReady();

        // Assert
        order.Status.Should().Be(ProductionStatus.Ready);
        order.ReadyAt.Should().NotBeNull();
        order.ReadyAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsDelivered_ShouldUpdateStatusAndSetDeliveredAt()
    {
        // Arrange
        var order = new ProductionOrder();
        order.StartProduction();
        order.MarkAsReady();

        // Act
        order.MarkAsDelivered();

        // Assert
        order.Status.Should().Be(ProductionStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();
        order.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetItems_ShouldSerializeItemsToJson()
    {
        // Arrange
        var order = new ProductionOrder();
        var items = new List<ProductionItem>
        {
            new ProductionItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "X-Burger",
                ProductCategory = "Lanche",
                Quantity = 2
            }
        };

        // Act
        order.SetItems(items);

        // Assert
        order.ItemsJson.Should().NotBeNullOrEmpty();
        order.ItemsJson.Should().Contain("X-Burger");
    }

    [Fact]
    public void GetItems_ShouldDeserializeItemsFromJson()
    {
        // Arrange
        var order = new ProductionOrder();
        var items = new List<ProductionItem>
        {
            new ProductionItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Fries",
                ProductCategory = "Acompanhamento",
                Quantity = 1
            },
            new ProductionItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Coke",
                ProductCategory = "Bebida",
                Quantity = 2
            }
        };
        order.SetItems(items);

        // Act
        var result = order.GetItems();

        // Assert
        result.Should().HaveCount(2);
        result[0].ProductName.Should().Be("Fries");
        result[1].ProductName.Should().Be("Coke");
    }

    [Fact]
    public void GetItems_WithInvalidJson_ShouldReturnEmptyList()
    {
        // Arrange
        var order = new ProductionOrder
        {
            ItemsJson = "invalid json"
        };

        // Act
        var result = order.GetItems();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ProductionOrder_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var orderId = Guid.NewGuid();
        var order = new ProductionOrder
        {
            OrderId = orderId,
            OrderNumber = "ORD-123",
            Priority = 5,
            EstimatedTime = 15
        };

        // Assert
        order.OrderId.Should().Be(orderId);
        order.OrderNumber.Should().Be("ORD-123");
        order.Priority.Should().Be(5);
        order.EstimatedTime.Should().Be(15);
    }
}
