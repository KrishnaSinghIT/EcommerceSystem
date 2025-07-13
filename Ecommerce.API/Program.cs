using Ecommerce.API.BackgroundJobs;
using Ecommerce.API.Middleware;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Observers;
using Ecommerce.Application.Services.Implementations;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Application.Validators;
using Ecommerce.Infrastructure.CommonPersitance;
using Ecommerce.Infrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddSingleton<IOrderStatusNotifier, OrderStatusNotifier>();
builder.Services.AddSingleton<IOrderStatusObserver, EmailNotifier>();
builder.Services.AddSingleton<IOrderStatusObserver, LoggerNotifier>();

builder.Services.AddHostedService<OrderFulfillmentService>();

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Register validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();
app.MapControllers();
//app.UseHttpsRedirection();

var notifier = app.Services.GetRequiredService<IOrderStatusNotifier>();
var emailObserver = app.Services.GetRequiredService<IOrderStatusObserver>();
notifier.Subscribe(emailObserver); // EmailNotifier
notifier.Subscribe(new LoggerNotifier()); // Manual instance


app.Run();
