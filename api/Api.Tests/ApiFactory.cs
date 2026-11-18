using Api.Sessions;
using Api.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FakeSessionStore Store { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISessionStore>();
            services.AddSingleton<ISessionStore>(Store);
        });
    }
}
