# StackFood - Production Service

Microserviço responsável por gerenciar a fila de produção, rastreamento de status e notificações de preparação de pedidos.

## Tecnologias

- **.NET 8.0** - Framework principal
- **PostgreSQL** - Banco de dados SQL com suporte a JSONB
- **Entity Framework Core 8.0** - ORM
- **AWS SNS** - Publicação de eventos
- **AWS SQS** - Consumo de mensagens de pedidos
- **LocalStack** - Emulação de serviços AWS localmente
- **Docker & Docker Compose** - Containerização

## Arquitetura

```
StackFood.Production/
├── src/
│   ├── StackFood.Production.API          # Controllers e configuração
│   ├── StackFood.Production.Application  # Use Cases, DTOs, Interfaces
│   ├── StackFood.Production.Domain       # Entidades, Enums, Eventos
│   └── StackFood.Production.Infrastructure # Repositórios, DbContext, AWS Services
└── tests/
    └── StackFood.Production.Tests        # Testes unitários e integração
```

## Domain Layer

### Entidades
- **ProductionOrder**: Pedido de produção com itens em JSON (JSONB)
- **ProductionItem**: Item individual do pedido

### Status do Pedido
```csharp
public enum ProductionStatus
{
    Received,    // Recebido na fila
    InProgress,  // Em preparação
    Ready,       // Pronto para retirada
    Delivered    // Entregue ao cliente
}
```

### Eventos Publicados
- **ProductionStartedEvent**: Produção iniciada
- **ProductionReadyEvent**: Pedido pronto
- **ProductionDeliveredEvent**: Pedido entregue

## Application Layer

### Use Cases
- **CreateProductionOrderUseCase**: Criar novo pedido na produção
- **GetProductionOrderUseCase**: Buscar pedido por ID ou OrderId
- **GetProductionQueueUseCase**: Obter fila de produção organizada
- **StartProductionUseCase**: Iniciar produção (publica evento)
- **UpdateProductionStatusUseCase**: Atualizar status (publica eventos)

### DTOs
- **ProductionOrderDTO**: DTO completo do pedido
- **ProductionQueueDTO**: DTO da fila (Received, InProgress, Ready)
- **CreateProductionOrderRequest**: Request para criar pedido
- **UpdateStatusRequest**: Request para atualizar status

## Infrastructure Layer

### PostgreSQL + EF Core
- **ProductionDbContext**: Contexto com mapeamento JSONB
- **ProductionRepository**: Repositório com queries otimizadas
- **Migrations**: Criação automática do schema

### AWS Services
- **SnsEventPublisher**: Publicação de eventos no SNS
- **OrderQueueConsumer**: Consumer SQS (Background Service)

## API Endpoints

### Production Queue
```http
GET /api/production/queue
```
Retorna a fila de produção organizada por status:
- **InQueue**: Pedidos aguardando (ordenados por prioridade e data)
- **InProgress**: Pedidos em preparação
- **Ready**: Pedidos prontos

### Production Orders
```http
POST /api/production/orders
GET /api/production/orders/{id}
GET /api/production/orders/order/{orderId}
PATCH /api/production/orders/{id}/status
```

## Configuração

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=production_db;Username=postgres;Password=postgres"
  },
  "AWS": {
    "Region": "us-east-1",
    "ServiceURL": "http://localstack:4566",
    "SNS": {
      "TopicArn": "arn:aws:sns:us-east-1:000000000000:sns-production-events"
    },
    "SQS": {
      "QueueUrl": "http://localstack:4566/000000000000/sqs-production-orders"
    }
  }
}
```

## Como Executar

### 1. Com Docker Compose (Recomendado)
```bash
# Subir todos os serviços
docker-compose up -d

# Ver logs
docker-compose logs -f production-api

# Parar serviços
docker-compose down
```

### 2. Localmente (Desenvolvimento)
```bash
# Instalar dependências
dotnet restore

# Aplicar migrations
dotnet ef database update --project src/StackFood.Production.Infrastructure/StackFood.Production.Infrastructure --startup-project src/StackFood.Production.API/StackFood.Production.API

# Executar API
dotnet run --project src/StackFood.Production.API/StackFood.Production.API
```

### 3. Criar nova Migration
```bash
dotnet ef migrations add MigrationName --project src/StackFood.Production.Infrastructure/StackFood.Production.Infrastructure --startup-project src/StackFood.Production.API/StackFood.Production.API --output-dir Data/Migrations
```

## Testes

```bash
# Executar todos os testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

## Integração com outros serviços

### Consome mensagens (SQS)
- **sqs-production-orders**: Recebe pedidos aprovados para produção

### Publica eventos (SNS)
- **sns-production-events**: Notifica mudanças de status
  - ProductionStarted
  - ProductionReady
  - ProductionDelivered

## Desenvolvimento

### Estrutura do Banco de Dados

```sql
CREATE TABLE production_orders (
    id uuid PRIMARY KEY,
    order_id uuid NOT NULL,
    order_number varchar(50) NOT NULL,
    status varchar(20) NOT NULL,
    items_json jsonb NOT NULL,
    priority int DEFAULT 1,
    estimated_time int,
    created_at timestamp NOT NULL,
    updated_at timestamp NOT NULL,
    started_at timestamp,
    ready_at timestamp,
    delivered_at timestamp
);

-- Índices
CREATE INDEX idx_production_order_id ON production_orders(order_id);
CREATE INDEX idx_production_status ON production_orders(status);
CREATE INDEX idx_production_status_created ON production_orders(status, created_at);
CREATE INDEX idx_production_queue ON production_orders(status, priority, created_at);
```

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Development` |
| `ConnectionStrings__DefaultConnection` | String de conexão PostgreSQL | `Host=localhost;Port=5432...` |
| `AWS__Region` | Região AWS | `us-east-1` |
| `AWS__ServiceURL` | URL do LocalStack | `http://localstack:4566` |
| `AWS__SNS__TopicArn` | ARN do tópico SNS | `arn:aws:sns:...` |
| `AWS__SQS__QueueUrl` | URL da fila SQS | `http://localstack:...` |

## Build Status

Compilação com êxito - 0 Erros, 2 Avisos

## Roadmap

- [ ] Implementar testes BDD com SpecFlow
- [ ] Adicionar métricas e observabilidade
- [ ] Implementar retry policy para eventos
- [ ] Dashboard em tempo real da fila
- [ ] Notificações push para clientes

## Licença

MIT
