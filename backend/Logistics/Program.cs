
using Logistics.Application.Abstractions;
using Logistics.Application.Ports;
using Logistics.Application.Services;
using Logistics.Api.Security;
using Logistics.Api.SignalR;
using Logistics.Infrastructure.Data;
using Logistics.Infrastructure.LoadOptimization;
using Logistics.Infrastructure.Masking;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.ReadModels;
using Logistics.Infrastructure.Storage;
using MediatR;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = HeaderIdentityAuthHandler.SchemeName;
                options.DefaultChallengeScheme = HeaderIdentityAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, HeaderIdentityAuthHandler>(
                HeaderIdentityAuthHandler.SchemeName,
                _ => { });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("DispatcherOnly", policy => policy.RequireRole("Dispatcher", "Admin"));
        });

        builder.Services.AddSignalR();
        builder.Services.AddHealthChecks();

        // CQRS via MediatR (handlers live in Application layer).
        builder.Services.AddMediatR(typeof(ApplicationAssembly).Assembly);

        builder.Services.AddDbContext<TmsDbContext>(options =>
        {
            var cs = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? builder.Configuration.GetConnectionString("TmsDb");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException(
                    "Missing connection string: set ConnectionStrings:DefaultConnection (or legacy TmsDb).");

            options.UseSqlServer(cs);
        });

        // Application ports implementations (Infrastructure)
        builder.Services.AddScoped<ILoadOptimizationDataProvider, LoadOptimizationDataProvider>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IRouteAssignmentService, RouteAssignmentService>();
        builder.Services.AddScoped<IDeliveryProofRepository, DeliveryProofRepository>();
        builder.Services.AddScoped<IDeliveryProofPhotoStorage, LocalDeliveryProofPhotoStorage>();
        builder.Services.AddScoped<LoadOptimizationService>();

        // PII masking + masked read-model factories
        builder.Services.AddScoped<IDataMaskingService, DataMaskingService>();
        builder.Services.AddScoped<IDriverPublicReadModelFactoryRaw, DriverPublicReadModelFactoryRaw>();
        builder.Services.AddScoped<IDriverPublicReadModelFactory, DriverPublicReadModelFactoryMaskedDecorator>();
        builder.Services.AddScoped<IExternalPublicReadModelFactoryRaw, ExternalPublicReadModelFactoryRaw>();
        builder.Services.AddScoped<IExternalPublicReadModelFactory, ExternalPublicReadModelFactoryMaskedDecorator>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);
        if (autoMigrate)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
            db.Database.Migrate();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Needed to serve uploaded delivery proof photos from LocalDeliveryProofPhotoStorage.
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        // Real-time tracking hub (implemented in later module).
        app.MapHub<TrackingHub>("/hubs/tracking");

        app.Run();
    }
}
