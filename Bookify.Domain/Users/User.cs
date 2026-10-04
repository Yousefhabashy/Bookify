using Bookify.Domain.Abstractions;
using Bookify.Domain.Users.Events;

namespace Bookify.Domain.Users
{
    public class User : Entity
    {
        private User(
            Guid id,
            FirstName firstName,
            LastName lastName,
            Email email
            ) : base(id)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public FirstName FirstName { get; private set; }
        public LastName LastName { get; private set; }
        public Email Email { get; private set; }

        public static Result<User> Create(
            Guid userId,
            FirstName firstName,
            LastName lastName,
            Email email
            )
        {
            if (firstName is null || lastName is null || email is null)
            {
                return Result.Failure<User>(new Error("User.InvalidDetails", "User details cannot be null."));
            }

            var user = new User(userId, firstName, lastName, email);

            user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id));
            return user;
        }
    }
}
