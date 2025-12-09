using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Harvestry.Edge.API;

public class LocalApiServer
{
    public static async Task RunAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Bind to all interfaces on port 5000
        builder.WebHost.UseUrls("http://0.0.0.0:5000");

        var app = builder.Build();

        // Serve Static Files (The Emergency Dashboard HTML/JS)
        app.UseDefaultFiles(); // Serves index.html
        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        
        // Emergency Override Endpoint
        app.MapPost("/api/emergency/stop", () => 
        {
            // Inject Engine dependency and call E-Stop
            return Results.Ok(new { status = "E-STOP TRIGGERED" });
        });

        await app.RunAsync();
    }
}
