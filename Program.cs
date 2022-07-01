using DockerWorkshops;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((hostBuilderContext, services, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();
    
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(nameof(SampleDbContext))));
        
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SampleDbContext>()
    .ForwardToPrometheus();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseRouting();
app.UseHttpMetrics();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapControllers();
app.MapMetrics();

if (string.Equals(Environment.GetEnvironmentVariable("RUN_MIGRATIONS_ON_STARTUP"), bool.TrueString, StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    Log.Information($"Running database migrations ({dbContext.GetType().Name})...");
    await dbContext.Database.MigrateAsync();
    Log.Information($"Migrations completed ({dbContext.GetType().Name}).");
}

app.Run();
