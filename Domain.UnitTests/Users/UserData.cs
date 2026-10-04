using Bookify.Domain.Users;

namespace Domain.UnitTests.Users
{
    internal static class UserData
    {
        public static FirstName ValidFirstName => FirstName.Create("John").Value;
        public static LastName ValidLastName => LastName.Create("Doe").Value;
        public static Email ValidEmail => Email.Create("john.doe@example.com").Value;
    }
}
