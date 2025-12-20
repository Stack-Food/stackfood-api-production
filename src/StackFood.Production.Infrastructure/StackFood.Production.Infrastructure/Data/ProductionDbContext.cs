using Microsoft.EntityFrameworkCore;
using StackFood.Production.Domain.Entities;
using StackFood.Production.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace StackFood.Production.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class ProductionDbContext : DbContext
{
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductionOrder> ProductionOrders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.ToTable("production_orders");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            entity.Property(e => e.OrderNumber)
                .HasColumnName("order_number")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<ProductionStatus>(v))
                .IsRequired();

            entity.Property(e => e.ItemsJson)
                .HasColumnName("items_json")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(e => e.EstimatedTime)
                .HasColumnName("estimated_time");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at");

            entity.Property(e => e.ReadyAt)
                .HasColumnName("ready_at");

            entity.Property(e => e.DeliveredAt)
                .HasColumnName("delivered_at");

            // Índices
            entity.HasIndex(e => e.OrderId)
                .HasDatabaseName("idx_production_order_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("idx_production_status");

            entity.HasIndex(e => new { e.Status, e.CreatedAt })
                .HasDatabaseName("idx_production_status_created");

            entity.HasIndex(e => new { e.Status, e.Priority, e.CreatedAt })
                .HasDatabaseName("idx_production_queue");
        });
    }
}
