using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pedidos.Application;

namespace Pedidos.Api.Controllers;

[ApiController, Route("auth")]
public sealed class AuthController(IAuthService authentication) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginInput input, CancellationToken cancellationToken)
    {
        var result = await authentication.LoginAsync(input, cancellationToken);
        return result is null ? Problem(statusCode: 401, title: "Correo o contrasena incorrectos.") : Ok(result);
    }
}