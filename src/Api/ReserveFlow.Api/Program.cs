using ReserveFlow.Api.Authentication;
using ReserveFlow.Booking.Application;
using ReserveFlow.Booking.Infrastructure;
using ReserveFlow.Booking.Presentation;
using ReserveFlow.Catalog.Application;
using ReserveFlow.Catalog.Infrastructure;
using ReserveFlow.Catalog.Presentation;
using ReserveFlow.Inventory.Application;
using ReserveFlow.Inventory.Infrastructure;
using ReserveFlow.Inventory.Presentation;
using ReserveFlow.Notifications.Infrastructure;
using ReserveFlow.Search.Application;
using ReserveFlow.Search.Application.Common;
using ReserveFlow.Search.Application.Common.Models;
using ReserveFlow.Search.Infrastructure;
using ReserveFlow.Search.Presentation.Properties;
using ReserveFlow.Shared.Application.Authentication;
using ReserveFlow.Shared.Infrastructure.Messaging.RabbitMq;
using Microsoft.EntityFrameworkCore;
using ReserveFlow.Booking.Infrastructure.Persistence;
using ReserveFlow.Catalog.Infrastructure.Persistence;
using ReserveFlow.Inventory.Infrastructure.Persistence;
using ReserveFlow.Notifications.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

string catalogConnectionString = builder.Configuration.GetConnectionString("CatalogConnection")
    ?? throw new InvalidOperationException("Catalog database connection string is not configured.");

string inventoryConnectionString = builder.Configuration.GetConnectionString("InventoryConnection")
    ?? throw new InvalidOperationException("Inventory database connection string is not configured.");

string bookingConnectionString = builder.Configuration.GetConnectionString("BookingConnection")
    ?? throw new InvalidOperationException("Booking database connection string is not configured.");

string notificationConnectionString = builder.Configuration.GetConnectionString("NotificationConnection")
    ?? throw new InvalidOperationException("Notification database connection string is not configured.");

// Shared infrastructure
builder.Services.AddRabbitMqInfrastructure(builder.Configuration);

// Catalog
builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(catalogConnectionString, builder.Configuration);

// Inventory
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(inventoryConnectionString);
builder.Services.AddInventoryPresentation();

// Booking
builder.Services.AddBookingApplication();
builder.Services.AddBookingInfrastructure(bookingConnectionString);
builder.Services.AddBookingPresentation();

// Notifications
builder.Services.AddNotificationInfrastructure(notificationConnectionString, builder.Configuration);

// Search
builder.Services.AddSearchApplication();
builder.Services.AddSearchInfrastructure(builder.Configuration);

// API
builder.Services.AddProblemDetails();

// Authentication is not implemented yet. The fixed development user is only
// registered in Development so the API cannot start unauthenticated elsewhere.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<ICurrentUser, DevelopmentCurrentUser>();
}
else
{
    throw new InvalidOperationException($"ICurrentUser is not configured for the '{builder.Environment.EnvironmentName}' environment. " + "Authentication must be implemented before running outside Development.");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();

    await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<BookingDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<NotificationsDbContext>().Database.MigrateAsync();
}

// Infrastructure initialization
await app.Services.InitializeBookingInfrastructureAsync();
await app.Services.InitializeNotificationInfrastructureAsync();
await app.Services.InitializeSearchInfrastructureAsync();
await app.Services.InitializeCatalogInfrastructureAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.MapCatalogEndpoints();
app.MapInventoryEndpoints();
app.MapSearchEndpoints();
app.MapBookingEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapGet(
        "/dev/elasticsearch-search-test",
        async (
            string? query,
            string? country,
            string? city,
            int? minGuests,
            int page,
            int pageSize,
            IPropertySearchRepository repository,
            CancellationToken cancellationToken) =>
        {
            var criteria = new PropertySearchCriteria(
                query,
                country,
                city,
                minGuests,
                page,
                pageSize);

            PropertySearchResult result =
                await repository.SearchAsync(
                    criteria,
                    cancellationToken);

            return Results.Ok(result);
        })
        .WithTags("Development");
}

app.Run();

public partial class Program
{ }
