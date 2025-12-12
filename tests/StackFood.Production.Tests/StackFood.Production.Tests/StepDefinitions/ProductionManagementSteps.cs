using FluentAssertions;
using Moq;
using StackFood.Production.Application.DTOs;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace StackFood.Production.Tests.StepDefinitions;

[Binding]
public class ProductionManagementSteps
{
    private readonly Mock<IProductionRepository> _repositoryMock;
    private Guid _orderId;
    private Guid _productionOrderId;
    private ProductionOrder? _productionOrder;
    private ProductionOrderDTO? _productionOrderResult;
    private ProductionQueueDTO? _queueResult;
    private List<ProductionOrder> _existingOrders = new();

    public ProductionManagementSteps()
    {
        _repositoryMock = new Mock<IProductionRepository>();
    }

    [Given(@"que recebi um pedido com id ""(.*)""")]
    public void GivenQueRecebiUmPedidoComId(string orderId)
    {
        _orderId = Guid.Parse(orderId);
    }

    [When(@"eu criar uma ordem de produção com os seguintes itens:")]
    public async Task WhenEuCriarUmaOrdemDeProducaoComOsSeguintesItens(Table table)
    {
        var items = new List<ProductionItemDTO>();

        foreach (var row in table.Rows)
        {
            items.Add(new ProductionItemDTO
            {
                ProductId = Guid.Parse(row["ProductId"]),
                ProductName = row["ProductName"],
                ProductCategory = row["ProductCategory"],
                Quantity = int.Parse(row["Quantity"])
            });
        }

        _productionOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = _orderId,
            OrderNumber = "ORD-001",
            Status = ProductionStatus.Received
        };
        _productionOrder.SetItems(items.Select(i => new ProductionItem
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductCategory = i.ProductCategory,
            Quantity = i.Quantity
        }).ToList());

        _productionOrderId = _productionOrder.Id;

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync(_productionOrder);

        var useCase = new CreateProductionOrderUseCase(_repositoryMock.Object);
        var request = new CreateProductionOrderRequest
        {
            OrderId = _orderId,
            OrderNumber = "ORD-001",
            Items = items
        };

        _productionOrderResult = await useCase.ExecuteAsync(request);
    }

    [Then(@"a ordem de produção deve ser criada")]
    public void ThenAOrdemDeProducaoDeveSerCriada()
    {
        _productionOrderResult.Should().NotBeNull();
        _productionOrderResult!.OrderId.Should().Be(_orderId);
    }

    [Then(@"o status deve ser ""(.*)""")]
    public void ThenOStatusDeveSer(string expectedStatus)
    {
        _productionOrderResult!.Status.Should().Be(expectedStatus);
    }

    [Given(@"que existe uma ordem de produção com status ""(.*)""")]
    public void GivenQueExisteUmaOrdemDeProducaoComStatus(string status)
    {
        _productionOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = Enum.Parse<ProductionStatus>(status)
        };
        _productionOrderId = _productionOrder.Id;

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_productionOrderId))
            .ReturnsAsync(_productionOrder);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductionOrder>()))
            .ReturnsAsync((ProductionOrder o) => o);
    }

    [When(@"eu iniciar a produção")]
    public async Task WhenEuIniciarAProducao()
    {
        var useCase = new StartProductionUseCase(_repositoryMock.Object, null!, null!);
        _productionOrderResult = await useCase.ExecuteAsync(_productionOrderId);
    }

    [Then(@"o status deve mudar para ""(.*)""")]
    public void ThenOStatusDeveMudarPara(string expectedStatus)
    {
        _productionOrderResult!.Status.Should().Be(expectedStatus);
    }

    [Then(@"a data de início deve ser registrada")]
    public void ThenADataDeInicioDeveSerRegistrada()
    {
        _productionOrderResult!.StartedAt.Should().NotBeNull();
    }

    [When(@"eu marcar como pronto")]
    public async Task WhenEuMarcarComoPronto()
    {
        var useCase = new UpdateProductionStatusUseCase(_repositoryMock.Object, null!, null!);
        var request = new UpdateStatusRequest { Status = "Ready" };
        _productionOrderResult = await useCase.ExecuteAsync(_productionOrderId, request);
    }

    [Then(@"a data de conclusão deve ser registrada")]
    public void ThenADataDeConclusaoDeveSerRegistrada()
    {
        _productionOrderResult!.ReadyAt.Should().NotBeNull();
    }

    [When(@"eu marcar como entregue")]
    public async Task WhenEuMarcarComoEntregue()
    {
        var useCase = new UpdateProductionStatusUseCase(_repositoryMock.Object, null!, null!);
        var request = new UpdateStatusRequest { Status = "Delivered" };
        _productionOrderResult = await useCase.ExecuteAsync(_productionOrderId, request);
    }

    [Given(@"que existem as seguintes ordens de produção:")]
    public void GivenQueExistemAsSeguintesOrdensDeProducao(Table table)
    {
        _existingOrders.Clear();

        foreach (var row in table.Rows)
        {
            var order = new ProductionOrder
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                OrderNumber = $"ORD-{_existingOrders.Count + 1:D3}",
                Status = Enum.Parse<ProductionStatus>(row["Status"]),
                Priority = int.Parse(row["Priority"])
            };

            if (order.Status == ProductionStatus.InProgress)
            {
                order.StartProduction();
            }
            else if (order.Status == ProductionStatus.Ready)
            {
                order.StartProduction();
                order.MarkAsReady();
            }

            _existingOrders.Add(order);
        }

        _repositoryMock
            .Setup(x => x.GetQueueAsync())
            .ReturnsAsync(_existingOrders);
    }

    [When(@"eu consultar a fila de produção")]
    public async Task WhenEuConsultarAFilaDeProducao()
    {
        var useCase = new GetProductionQueueUseCase(_repositoryMock.Object);
        _queueResult = await useCase.ExecuteAsync();
    }

    [Then(@"devo ver (.*) pedidos? na fila ""(.*)""")]
    public void ThenDevoVerPedidosNaFila(int expectedCount, string queueName)
    {
        _queueResult.Should().NotBeNull();

        switch (queueName)
        {
            case "Received":
                _queueResult!.InQueue.Should().HaveCount(expectedCount);
                break;
            case "InProgress":
                _queueResult!.InProgress.Should().HaveCount(expectedCount);
                break;
            case "Ready":
                _queueResult!.Ready.Should().HaveCount(expectedCount);
                break;
        }
    }
}
