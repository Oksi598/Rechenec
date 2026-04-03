using System.Net.Http.Json;
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

namespace Logistics.Api.Tests;

public sealed class MaskingScopeIntegrationTests
{
    [Fact]
    public async Task Anonymous_Request_Uses_External_MaskingScope()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var id = Guid.NewGuid();
        var dto = await client.GetFromJsonAsync<DriverPublicDto>($"/api/tms/drivers/{id}/public");

        Assert.NotNull(dto);
        Assert.Equal(MaskingScope.External.ToString(), dto!.FullName);
    }

    [Fact]
    public async Task Dispatcher_Header_Uses_Internal_MaskingScope()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("X-User-Role", "Dispatcher");

        var id = Guid.NewGuid();
        var dto = await client.GetFromJsonAsync<DriverPublicDto>($"/api/tms/drivers/{id}/public");

        Assert.NotNull(dto);
        Assert.Equal(MaskingScope.Internal.ToString(), dto!.FullName);
    }

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:AutoMigrate"] = "false"
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
