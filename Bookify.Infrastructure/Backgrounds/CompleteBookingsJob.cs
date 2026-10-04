using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.AutoCompleteBooking;
using Bookify.Domain.Bookings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Bookify.Infrastructure.Backgrounds
{
    [DisallowConcurrentExecution]
    internal class CompleteBookingsJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CompleteBookingsJob> _logger;

        public CompleteBookingsJob(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CompleteBookingsJob> logger
            )
        {
            _scopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting to complete bookings job");
            using var scope = _scopeFactory.CreateScope();

            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var utcNow = dateTimeProvider.UtcNow;
            DateOnly today = DateOnly.FromDateTime(utcNow);

            var bookingsToComplete = await bookingRepository.GetCompletableBookingsAsync(
               today, context.CancellationToken);

            if (!bookingsToComplete.Any())
            {
                return;
            }

            foreach (var bookingId in bookingsToComplete)
            {
                try
                {
                    var command = new AutoCompleteBookingCommand(bookingId);
                    var result = await sender.Send(command, cancellationToken);

                    if (result.IsFailure)
                    {
                        _logger.LogWarning("Failed to complete booking {BookingId}: {Error}", bookingId, result.Error.Code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while completing booking {BookingId}", bookingId);
                }
            }
            _logger.LogInformation("Completed Complete bookings job. Completed {Count} bookings", bookingsToComplete.Count);
        }
    }
}