namespace Bookify.Application.Abstractions
{
    public interface IUserContext
    {
        bool IsAuthenticated { get; }
        public bool IsAdmin { get; }
        Guid UserId { get; }
        string Email { get; }
        string FirstName { get; }
        string LastName { get; }
    }
}
