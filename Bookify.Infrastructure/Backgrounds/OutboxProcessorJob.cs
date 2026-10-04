using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Events;
using Bookify.Domain.Abstractions;
using Bookify.Infrastructure.Abstractions.Outbox;
using Bookify.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.Json;

namespace Bookify.Infrastructure.Backgrounds
{
    [DisallowConcurrentExecution]
    internal sealed class OutboxProcessorJob : IJob
    {
        private const int BatchSize = 20;
        private const int MaxRetries = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessorJob> _logger;

        public OutboxProcessorJob(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null && m.RetryCount < MaxRetries)
                .OrderBy(m => m.RetryCount)
                .ThenBy(m => m.OccurredOnUtc)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

            foreach (var message in messages)
            {
                await ProcessMessageAsync(message, publisher, context.CancellationToken, dateTimeProvider);

                // for saving message by message (Processed or Failed)
                await dbContext.SaveChangesAsync(context.CancellationToken);
            }

        }

        private async Task ProcessMessageAsync(
            OutboxMessage message,
            IPublisher publisher,
            CancellationToken cancellationToken,
            IDateTimeProvider dateTimeProvider
            )
        {
            try
            {
                var domainEventType = DomainEventRegistry.GetEventType(message.Type)
                   ?? throw new InvalidOperationException($"Could not resolve registered type for '{message.Type}'");

                var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Content, domainEventType)
                        ?? throw new InvalidOperationException("Failed to deserialize domain event");

                var notification = DomainEventNotificationFactory.Wrap(domainEvent);

                await publisher.Publish(notification, cancellationToken);

                message.ProcessedOnUtc = dateTimeProvider.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.ToString();

                if (message.RetryCount >= MaxRetries)
                {
                    _logger.LogCritical(
                        ex,
                        "Outbox message {MessageId} ({Type}) exceeded {MaxRetries} retries and will not be processed again",
                        message.Id, message.Type, MaxRetries);
                }
                else
                {
                    _logger.LogError(
                        ex,
                        "Failed to process outbox message {MessageId} (attempt {Attempt}/{MaxRetries})",
                        message.Id, message.RetryCount, MaxRetries);
                }
            }
        }
    }
}
