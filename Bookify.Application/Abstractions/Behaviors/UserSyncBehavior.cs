using Bookify.Application.Abstractions.Exceptions;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Abstractions.Behaviors
{
    public sealed class UserSyncBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IUserContext _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserSyncBehavior<TRequest, TResponse>> _logger;

        public UserSyncBehavior(
            IUserContext userContext,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IDistributedCache cache,
            ILogger<UserSyncBehavior<TRequest, TResponse>> logger)
        {
            _userContext = userContext;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not IBaseCommand)
            {
                return await next();
            }

            if (_userContext.IsAuthenticated)
            {
                await EnsureUserSyncedAsync(cancellationToken);
            }

            return await next();
        }

        private async Task EnsureUserSyncedAsync(CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            var cacheKey = $"user-synced:{userId}";

            var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (cachedValue is not null)
            {
                return;
            }

            var exists = await _userRepository.ExistsAsync(userId, cancellationToken);

            if (!exists)
            {
                if (string.IsNullOrWhiteSpace(_userContext.Email) ||
                    string.IsNullOrWhiteSpace(_userContext.FirstName) ||
                    string.IsNullOrWhiteSpace(_userContext.LastName))
                {
                    throw new UserProfileIncompleteException("User profile is incomplete. Email, First Name, and Last Name are required from the Identity Provider.");
                }

                var email = Email.Create(_userContext.Email);
                var firstName = FirstName.Create(_userContext.FirstName);
                var lastName = LastName.Create(_userContext.LastName);

                if (email.IsFailure || firstName.IsFailure || lastName.IsFailure)
                {
                    throw new UserProfileIncompleteException(
                        "User profile from the Identity Provider is missing or has invalid Email, First Name, or Last Name.");
                }

                var user = User.Create(userId, firstName.Value, lastName.Value, email.Value);
                if (user.IsFailure)
                {
                    throw new UserProfileIncompleteException(user.Error.Message);
                }

                _userRepository.Add(user.Value);

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Synced new user {UserId} from identity provider", userId);
                }
                catch (DbUpdateException ex)
                {
                    _unitOfWork.ClearChanges();

                    if (!await _userRepository.ExistsAsync(userId, cancellationToken))
                    {
                        throw;
                    }

                    _logger.LogInformation(ex, "User {UserId} was created by a concurrent request", userId);
                }
            }

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(cacheKey, "1", cacheOptions, cancellationToken);
        }
    }
}