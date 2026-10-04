using Application.IntegrationTests.Infrastructure;
using Bookify.Infrastructure.Abstractions.Outbox;
using Bookify.Infrastructure.Backgrounds;
using Docker.DotNet.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Quartz;

namespace Application.IntegrationTests.Outbox
{
    public class OutboxTests : BaseIntegrationTest
    {
        public OutboxTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        private OutboxProcessorJob CreateJob() => new(
            Factory.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxProcessorJob>.Instance);

        private static async Task RunAsync(OutboxProcessorJob job) =>
            await job.Execute(Substitute.For<IJobExecutionContext>(), CancellationToken.None);

        private Task<OutboxMessage> LoadAsync(Guid id) =>
            DbContext.OutboxMessages.AsNoTracking().SingleAsync(m => m.Id == id);

        [Fact]
        public async Task Reserve_ShouldWriteAnOutboxMessage_AndTheJobShouldMarkItProcessed()
        {
            await CreateUserAsync();
            var apartmentId = await CreateApartmentAsync();
            await ClearOutboxAsync();

            var bookingId = await ReserveAsync(apartmentId, PeriodFromToday(30, 3));

            var pending = await DbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(m => m.Type == "booking-reserved");

            pending.Content.Should().Contain(bookingId.ToString());
            pending.ProcessedOnUtc.Should().BeNull();

            await RunAsync(CreateJob());

            var processed = await LoadAsync(pending.Id);
            processed.ProcessedOnUtc.Should().NotBeNull();
            processed.Error.Should().BeNull();
        }

        [Fact]
        public async Task Job_ShouldStopRetryingAMessageThatKeepsFailing_AfterFiveAttempts()
        {
            await ClearOutboxAsync();

            var id = Guid.NewGuid();
            DbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = id,
                Type = "no-such-event",
                Content = "{}",
                OccurredOnUtc = DateTime.UtcNow
            });
            await DbContext.SaveChangesAsync();

            var job = CreateJob();

            for (var attempt = 1; attempt <= 5; attempt++)
            {
                await RunAsync(job);

                var message = await LoadAsync(id);
                message.RetryCount.Should().Be(attempt);
                message.ProcessedOnUtc.Should().BeNull();
            }

            await RunAsync(job);   

            var final = await LoadAsync(id);
            final.RetryCount.Should().Be(5);
            final.ProcessedOnUtc.Should().BeNull();
            final.Error.Should().NotBeNullOrEmpty(); 
        }
    }
}