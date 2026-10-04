using Bookify.Application.Users.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Users
{
    [Route("api/users")]
    public class UsersController : ApiController
    {
        public UsersController(ISender sender) : base(sender)
        {
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserRequest request,
            CancellationToken cancellationToken
            )
        {
            var command = new RegisterCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName
                );

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess ? Created(string.Empty, result.Value) : HandleFailure(result);
        }
    }
}
