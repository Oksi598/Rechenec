using System.Text;
using Logistics.Api.Data;
using Logistics.Domain;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Logistics.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true);
            });
        });

        var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32)
            throw new InvalidOperationException(
                $"Jwt:{nameof(JwtOptions.SigningKey)} must be at least 32 characters.");

        builder.Services.Configure<JwtOptions>(jwtSection);
        builder.Services.AddSingleton<JwtTokenBuilder>();

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<TmsDbContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "DriverOrDispatcher",
                policy => policy.RequireRole(AppRoles.Driver, AppRoles.Dispatcher));
        });

        builder.Services.AddSignalR();
        builder.Services.AddHealthChecks();

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

        builder.Services.AddScoped<ILoadOptimizationDataProvider, LoadOptimizationDataProvider>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IRouteAssignmentService, RouteAssignmentService>();
        builder.Services.AddScoped<IDeliveryProofRepository, DeliveryProofRepository>();
        builder.Services.AddScoped<IDeliveryProofPhotoStorage, LocalDeliveryProofPhotoStorage>();
        builder.Services.AddScoped<LoadOptimizationService>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

        builder.Services.AddScoped<IDataMaskingService, DataMaskingService>();
        builder.Services.AddScoped<IDriverPublicReadModelFactoryRaw, DriverPublicReadModelFactoryRaw>();
        builder.Services.AddScoped<IDriverPublicReadModelFactory, DriverPublicReadModelFactoryMaskedDecorator>();
        builder.Services.AddScoped<IExternalPublicReadModelFactoryRaw, ExternalPublicReadModelFactoryRaw>();
        builder.Services.AddScoped<IExternalPublicReadModelFactory, ExternalPublicReadModelFactoryMaskedDecorator>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Logistics TMS API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);
        if (autoMigrate)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
            await db.Database.MigrateAsync();
        }

        if (!app.Environment.IsEnvironment("Testing"))
            await DataSeeder.SeedAsync(app.Services);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapHub<TrackingHub>("/hubs/tracking");

        await app.RunAsync();
    }
}
