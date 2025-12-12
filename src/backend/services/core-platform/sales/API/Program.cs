using Harvestry.Packages.Application.Interfaces;
using Harvestry.Packages.Infrastructure.Persistence;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Application.Services;
using Harvestry.Sales.Infrastructure.Persistence;
using Harvestry.Sales.Infrastructure.Persistence.Rls;
using Harvestry.Sales.Infrastructure.Repositories;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== Authentication & Authorization =====
var supabaseConfigured = builder.Configuration.GetSection("Supabase").Exists()
    && !string.IsNullOrWhiteSpace(builder.Configuration["Supabase:JwtSecret"]);

if (supabaseConfigured)
{
    builder.Services.AddSupabaseJwtAuthentication(builder.Configuration);
}
else if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("Header")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
            Harvestry.Shared.Authentication.HeaderAuthenticationHandler>("Header", null);
}
else
{
    throw new InvalidOperationException("Supabase authentication must be configured in production.");
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ===== RLS context + EF connection interceptor =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISiteRlsContextAccessor, HttpSiteRlsContextAccessor>();
builder.Services.AddScoped<NpgsqlRlsConnectionInterceptor>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("Sales")
    ?? throw new InvalidOperationException("Database connection string not found.");

builder.Services.AddDbContext<SalesDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(sp.GetRequiredService<NpgsqlRlsConnectionInterceptor>());
});

builder.Services.AddDbContext<PackagesDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(sp.GetRequiredService<NpgsqlRlsConnectionInterceptor>());
});

// ===== Repositories =====
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IComplianceEventRepository, ComplianceEventRepository>();
builder.Services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
builder.Services.AddScoped<ISalesOrderLineRepository, SalesOrderLineRepository>();
builder.Services.AddScoped<ISalesAllocationRepository, SalesAllocationRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentPackageRepository, ShipmentPackageRepository>();

builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IMovementRepository, MovementRepository>();

// ===== Services =====
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IComplianceEventService, ComplianceEventService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;

