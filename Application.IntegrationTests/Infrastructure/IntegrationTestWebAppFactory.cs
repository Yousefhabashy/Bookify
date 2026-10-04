using Application.IntegrationTests.Context;
using Bookify.Api;
using Bookify.Application.Abstractions;
using Bookify.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace Application.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17")   // نفس نسخة docker-compose
            .Build();

        public TestUserContext UserContext { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(o =>
                    o.UseNpgsql(_postgres.GetConnectionString()).UseSnakeCaseNamingConvention());


                services.RemoveAll<IDistributedCache>();
                services.AddDistributedMemoryCache();


                services.RemoveAll<IUserContext>();
                services.AddSingleton<IUserContext>(UserContext);

                services.RemoveAll<IHostedService>();
            });
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
