
using Bookify.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.IntegrationTests.Context
{
    public sealed class TestUserContext : IUserContext
    {
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = "test@bookify.com";
        public string FirstName { get; set; } = "Test";
        public string LastName { get; set; } = "User";
    }
}
