using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Reviews.GetReviewsByApartmentId
{
    internal sealed class GetReviewsByApartmentIdQueryHandler : IQueryHandler<GetReviewsByApartmentIdQuery, IReadOnlyCollection<ReviewResponse>>
    {
        private readonly IApplicationDbContext _context;
        public GetReviewsByApartmentIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IReadOnlyCollection<ReviewResponse>>> Handle(
            GetReviewsByApartmentIdQuery request,
            CancellationToken cancellationToken
            )
        {
            var reviews = await _context.Reviews
            .Where(r => r.ApartmentId == request.ApartmentId)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                Rating = r.Rating.Value,
                Comment = r.Comment.Value,
                CreatedOnUtc = r.CreatedOnUtc
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyCollection<ReviewResponse>>(reviews);
        }
    }
}
