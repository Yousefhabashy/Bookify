namespace Bookify.Application.Abstractions.Exceptions
{
    public sealed class UserProfileIncompleteException : Exception
    {
        public UserProfileIncompleteException(string message) : base(message)
        {
        }
    }
}
