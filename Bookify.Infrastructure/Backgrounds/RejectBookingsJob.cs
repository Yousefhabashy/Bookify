using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.AutoRejectBooking;
using Bookify.Domain.Bookings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Bookify.Infrastructure.Backgrounds
{
    [DisallowConcurrentExecution]
    internal sealed class RejectBookingsJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RejectBookingsJob> _logger;

        public RejectBookingsJob(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RejectBookingsJob> logger
            )
        {
            _scopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting to reject bookings job");
            using var scope = _scopeFactory.CreateScope();

            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            var threshold = dateTimeProvider.UtcNow.AddMinutes(-15);
            var bookingsToReject = await bookingRepository.RejectPendingBookingsJobAsync(
                threshold, context.CancellationToken);

            if (!bookingsToReject.Any())
            {
                return;
            }

            foreach (var bookingId in bookingsToReject)
            {
                try
                {
                    var command = new AutoRejectBookingCommand(bookingId);
                    var result = await sender.Send(command, cancellationToken);

                    if (result.IsFailure)
                    {
                        _logger.LogWarning("Failed to reject booking {BookingId}: {Error}", bookingId, result.Error.Code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while rejecting booking {BookingId}", bookingId);
                }
            }
            _logger.LogInformation("Completed rejecting bookings job. Rejected {Count} bookings", bookingsToReject.Count);
        }
    }
}
