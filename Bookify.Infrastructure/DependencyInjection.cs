using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Authentication;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Data;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Reviews;
using Bookify.Domain.Users;
using Bookify.Infrastructure.Authentication;
using Bookify.Infrastructure.Backgrounds;
using Bookify.Infrastructure.Caching;
using Bookify.Infrastructure.Clock;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Bookify.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Database"))
                       .UseSnakeCaseNamingConvention());

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));
            services.AddHttpClient<IAuthenticationService, KeycloakAuthenticationService>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["Keycloak:AdminUrl"]!);
            });

            services.AddTransient<
                Microsoft.AspNetCore.Authentication.IClaimsTransformation,
                KeycloakRolesClaimsTransformation>();

            // distributed caching
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });
            services.Decorate<IDistributedCache, ResilientDistributedCache>();

            services.AddScoped<IUserContext, UserContext>();

            services.AddScoped<IApartmentRepository, ApartmentRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            // Quartz
            services.AddQuartz(configurator =>
            {
                var jobKey = new JobKey(nameof(RejectBookingsJob));

                configurator.AddJob<RejectBookingsJob>(opts => opts.WithIdentity(jobKey));
                configurator.AddTrigger(trigger => trigger
                    .ForJob(jobKey)
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromMinutes(1))
                        .RepeatForever()));

                var outboxProcessorJobKey = new JobKey(nameof(OutboxProcessorJob));
                configurator.AddJob<OutboxProcessorJob>(opts => opts.WithIdentity(outboxProcessorJobKey));
                configurator.AddTrigger(trigger => trigger
                    .ForJob(outboxProcessorJobKey)
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromSeconds(30))
                        .RepeatForever()));

                var confirmBookingsJobKey = new JobKey(nameof(CompleteBookingsJob));
                configurator.AddJob<CompleteBookingsJob>(opts => opts.WithIdentity(confirmBookingsJobKey));
                configurator.AddTrigger(trigger => trigger
                    .ForJob(confirmBookingsJobKey)
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromMinutes(30))
                        .RepeatForever()));
            });
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            return services;
        }
    }
}
