using Bookify.Application.Abstractions.Events;
using Bookify.Domain.Bookings.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;

namespace Bookify.Application.Bookings.ConfirmBooking.EventHandler
{
    internal sealed class BookingConfirmedDomainEventHandler : INotificationHandler<DomainEventNotification<BookingConfirmedDomainEvent>>
    {
        private readonly ILogger<BookingConfirmedDomainEventHandler> _logger;

        public BookingConfirmedDomainEventHandler(ILogger<BookingConfirmedDomainEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(
            DomainEventNotification<BookingConfirmedDomainEvent> notification,
            CancellationToken cancellationToken)
        {
            // future implementation: send an email to the user notifying them that their booking has been confirmed
            _logger.LogInformation(
                "Booking {BookingId} was confirmed. Notifying the user (placeholder).",
                notification.DomainEvent.BookingId);

            return Task.CompletedTask;
        }
    }
}
