using Ecommerce.API.Extensions;
using Ecommerce.API.Middleware;
using Ecommerce.Application.Observers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

//  Clean DI registration
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddObservability(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapHealthChecks("/health");


var notifier = app.Services.GetRequiredService<IOrderStatusNotifier>();
var emailObserver = app.Services.GetRequiredService<IOrderStatusObserver>();
notifier.Subscribe(emailObserver); 
notifier.Subscribe(new LoggerNotifier()); 


app.Run();
public partial class Program { }
