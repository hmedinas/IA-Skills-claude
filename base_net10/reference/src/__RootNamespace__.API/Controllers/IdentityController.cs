using __RootNamespace__.Application.Auth;
using __RootNamespace__.Domain.Auth;

namespace __RootNamespace__.API.Controllers;

[SwaggerTag("Autenticación: login y renovación de token JWT")]
public class IdentityController(ILogger<IdentityController> logger) : BaseApiController(logger)
{
    [HttpPost("login")]
    public async Task<ActionResult<Response<object>>> Login(UserLoginCommand command)
        => HandleResponse(await Mediator.Send(new LoginCommand(command)));

    [HttpPut("refresh")]
    public async Task<ActionResult<Response<object>>> Refresh(RefreshTokenCommand command)
        => HandleResponse(await Mediator.Send(new RefreshTokenQuery(command)));
}
