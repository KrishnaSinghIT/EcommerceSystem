using Ecommerce.API.BackgroundJobs;
using Ecommerce.Application.BackgroundQueue;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Observers;
using Ecommerce.Application.Services.Implementations;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Application.Services.Monitoring;
using Ecommerce.Application.Validators;
using Ecommerce.Infrastructure.CommonPersitance;
using Ecommerce.Infrastructure.Data;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ecommerce.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repositories and UoW
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Services
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();

            // Observers
            services.AddSingleton<IOrderStatusNotifier, OrderStatusNotifier>();
            services.AddSingleton<IOrderStatusObserver, EmailNotifier>();
            services.AddSingleton<IOrderStatusObserver, LoggerNotifier>();

            // Queue + Background
            services.AddSingleton<IOrderProcessingQueue, OrderProcessingQueue>();
            services.AddHostedService<OrderFulfillmentService>();

            // Metrics
            services.AddSingleton<IMetricsService, MetricsService>();

            // Validation
            services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

            // Caching
            services.AddMemoryCache();

            return services;
        }

        public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("EcommerceSystem"))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddConsoleExporter(); //  we can also use AddZipkinExporter for UI 
                });

            services.AddHealthChecks()
                .AddSqlServer(
                    configuration.GetConnectionString("DefaultConnection")!,
                    name: "sql",
                    failureStatus: HealthStatus.Unhealthy
                );

            return services;
        }
    }
}
