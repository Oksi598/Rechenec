
using Logistics.Application.Abstractions;
using Logistics.Application.Ports;
using Logistics.Application.Services;
using Logistics.Api.SignalR;
using Logistics.Infrastructure.Data;
using Logistics.Infrastructure.LoadOptimization;
using Logistics.Infrastructure.Masking;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.ReadModels;
using Logistics.Infrastructure.Storage;
using MediatR;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddAuthorization();

        builder.Services.AddSignalR();

        // CQRS via MediatR (handlers live in Application layer).
        builder.Services.AddMediatR(typeof(ApplicationAssembly).Assembly);

        // EF Core (SQL Server + NetTopologySuite)
        builder.Services.AddDbContext<TmsDbContext>(options =>
        {
            var cs = builder.Configuration.GetConnectionString("TmsDb");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Missing connection string: ConnectionStrings:TmsDb");

            options.UseSqlServer(cs, sql => sql.UseNetTopologySuite());
        });

        // Application ports implementations (Infrastructure)
        builder.Services.AddScoped<ILoadOptimizationDataProvider, LoadOptimizationDataProvider>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Needed to serve uploaded delivery proof photos from LocalDeliveryProofPhotoStorage.
        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapControllers();

        // Real-time tracking hub (implemented in later module).
        app.MapHub<TrackingHub>("/hubs/tracking");

        app.Run();
    }
}
