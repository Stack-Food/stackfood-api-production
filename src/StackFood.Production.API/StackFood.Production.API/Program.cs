using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using StackFood.Production.Application.Configuration;
using StackFood.Production.Application.Interfaces;
using StackFood.Production.Application.UseCases;
using StackFood.Production.Infrastructure.Data;
using StackFood.Production.Infrastructure.Repositories;
using StackFood.Production.Infrastructure.Services;
using System.Diagnostics.CodeAnalysis;

namespace StackFood.Production.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Application Settings
            builder.Services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new ProductionSettings
                {
                    SnsTopicArn = config["AWS:SNS:TopicArn"] ?? "arn:aws:sns:us-east-1:000000000000:sns-production-events"
                };
            });

            // PostgreSQL
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=production_db;Username=postgres;Password=postgres";

            builder.Services.AddDbContext<ProductionDbContext>(options =>
                options.UseNpgsql(connectionString));

            // AWS Services
            builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
            builder.Services.AddAWSService<IAmazonSQS>();

            // Repositories
            builder.Services.AddScoped<IProductionRepository, ProductionRepository>();

            // Services
            builder.Services.AddScoped<IEventPublisher, SnsEventPublisher>();

            // Use Cases
            builder.Services.AddScoped<CreateProductionOrderUseCase>();
            builder.Services.AddScoped<GetProductionOrderUseCase>();
            builder.Services.AddScoped<GetProductionQueueUseCase>();
            builder.Services.AddScoped<StartProductionUseCase>();
            builder.Services.AddScoped<UpdateProductionStatusUseCase>();

            // Background Services
            builder.Services.AddHostedService<OrderQueueConsumer>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}