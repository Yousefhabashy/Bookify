using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Reviews;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Reviews.GetReviewById
{
    internal sealed class GetReviewByIdQueryHandler : IQueryHandler<GetReviewByIdQuery, ReviewResponse>
    {
        private readonly IApplicationDbContext _context;

        public GetReviewByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ReviewResponse>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
        {
            var review = await _context.Reviews
                .Where(r => r.Id == request.ReviewId)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Comment = r.Comment.Value,
                    Rating = r.Rating.Value,
                    CreatedOnUtc = r.CreatedOnUtc
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (review is null)
                return Result.Failure<ReviewResponse>(ReviewErrors.NotFound);

            return review;
        }
    }
}
