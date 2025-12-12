using Harvestry.Compliance.Metrc.Infrastructure;
using Harvestry.Packages.Infrastructure.Persistence;
using Harvestry.Sales.Infrastructure.Persistence;
using Harvestry.Sales.Infrastructure.Persistence.Rls;
using Harvestry.Shared.Authentication;
using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Application.Services;
using Harvestry.Transfers.Infrastructure.Persistence;
using Harvestry.Transfers.Infrastructure.Repositories;
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
    ?? builder.Configuration.GetConnectionString("Transfers")
    ?? throw new InvalidOperationException("Database connection string not found.");

builder.Services.AddDbContext<TransfersDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(sp.GetRequiredService<NpgsqlRlsConnectionInterceptor>());
});

// Read-only contexts for source-of-truth data
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

// METRC queue (no worker in this API host)
builder.Services.AddMetrcComplianceWithoutWorker(builder.Configuration);

// ===== Repositories =====
builder.Services.AddScoped<IOutboundTransferRepository, OutboundTransferRepository>();
builder.Services.AddScoped<IOutboundTransferPackageRepository, OutboundTransferPackageRepository>();
builder.Services.AddScoped<ITransferEventRepository, TransferEventRepository>();
builder.Services.AddScoped<ITransportManifestRepository, TransportManifestRepository>();
builder.Services.AddScoped<IShipmentTransferSourceReader, ShipmentTransferSourceReader>();
builder.Services.AddScoped<IInboundReceiptRepository, InboundReceiptRepository>();
builder.Services.AddScoped<IInboundReceiptLineRepository, InboundReceiptLineRepository>();

// ===== Services =====
builder.Services.AddScoped<IOutboundTransferService, OutboundTransferService>();
builder.Services.AddScoped<ITransportManifestService, TransportManifestService>();
builder.Services.AddScoped<IInboundTransferReceiptService, InboundTransferReceiptService>();

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

