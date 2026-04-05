using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Logistics.Api;
using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using Logistics.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Logistics.Api.Tests;

public sealed class MaskingScopeIntegrationTests
{
    private const string TestJwtKey = "UnitTestSigningKey_MustBe32CharsMin!!!!!!!!";

    [Fact]
    public async Task Unauthenticated_Request_Returns_Unauthorized()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/tms/drivers/{Guid.NewGuid()}/public");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Customer_Jwt_Uses_External_MaskingScope()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateTestJwt(Guid.NewGuid(), "Customer"));

        var id = Guid.NewGuid();
        var dto = await client.GetFromJsonAsync<DriverPublicDto>($"/api/tms/drivers/{id}/public");

        Assert.NotNull(dto);
        Assert.Equal(MaskingScope.External.ToString(), dto!.FullName);
    }

    [Fact]
    public async Task Dispatcher_Jwt_Uses_Internal_MaskingScope()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateTestJwt(Guid.NewGuid(), "Dispatcher"));

        var id = Guid.NewGuid();
        var dto = await client.GetFromJsonAsync<DriverPublicDto>($"/api/tms/drivers/{id}/public");

        Assert.NotNull(dto);
        Assert.Equal(MaskingScope.Internal.ToString(), dto!.FullName);
    }

    private static string CreateTestJwt(Guid userId, string role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            "Logistics.Tms.Tests",
            "Logistics.Tms.Tests",
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:AutoMigrate"] = "false",
                    ["Jwt:Issuer"] = "Logistics.Tms.Tests",
                    ["Jwt:Audience"] = "Logistics.Tms.Tests",
                    ["Jwt:SigningKey"] = TestJwtKey,
                    ["Jwt:AccessTokenMinutes"] = "60"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton<IMediator, FakeMediator>();
            });
        }
    }

    private sealed class FakeMediator : IMediator
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetDriverPublicDtoQuery driverQuery)
            {
                var result = new DriverPublicDto(
                    driverQuery.DriverId,
                    driverQuery.Scope.ToString(),
                    new MaskedContactDto("masked@example.com", "+38097****567"));

                return Task.FromResult((TResponse)(object)result);
            }

            if (request is GetExternalContactDtoQuery contactQuery)
            {
                var result = new ExternalContactDto(
                    contactQuery.ExternalContactId,
                    contactQuery.Scope.ToString(),
                    new MaskedContactDto("masked@example.com", "+38097****567"));

                return Task.FromResult((TResponse)(object)result);
            }

            throw new InvalidOperationException($"Unexpected request: {request.GetType().Name}");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => EmptyAsync<TResponse>();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
            => EmptyAsync<object?>();

        private static async IAsyncEnumerable<T> EmptyAsync<T>()
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
