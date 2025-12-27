namespace StackFood.Production.Domain.Enums;

public enum ProductionStatus
{
    Received,      // Recebido na fila
    InProgress,    // Em preparação
    Ready,         // Pronto para retirada
    Delivered      // Entregue ao cliente
}
