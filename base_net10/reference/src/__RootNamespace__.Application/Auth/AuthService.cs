namespace __RootNamespace__.Application.Auth;

public record LoginCommand(UserLoginCommand Credentials) : IRequest<Response<object>>;
public record RefreshTokenQuery(RefreshTokenCommand Tokens) : IRequest<Response<object>>;

public class AuthHandler(IAuthRepository repository, JwtHandler jwtHandler, IConfiguration config) :
    IRequestHandler<LoginCommand, Response<object>>,
    IRequestHandler<RefreshTokenQuery, Response<object>>
{
    public async Task<Response<object>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (usuario, _, mensaje) = await repository.ValidateCredentialsAsync(request.Credentials.UserName, request.Credentials.Password);

            return usuario == null
                ? new Response<object>(TypeTransaction.ERR, mensaje ?? "Usuario o contraseña incorrectos.")
                : new Response<object>(TypeTransaction.OK, BuildTokenPayload(usuario));
        }
        catch (Exception ex)
        {
            return new Response<object>(TypeTransaction.ERR, ex.Message);
        }
    }

    public async Task<Response<object>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var jwt = JwtHandler.ReadToken(request.Tokens.Token);

            var expiresRefresh = config.GetValue<int>("Jwt:ExpiresRefresh");
            if (jwt.ValidTo.AddMinutes(expiresRefresh) < DateTime.UtcNow)
                return new Response<object>(TypeTransaction.ERR, "El token ha expirado, inicie sesión nuevamente.");

            var userDataClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;
            if (userDataClaim == null)
                return new Response<object>(TypeTransaction.ERR, "Token inválido.");

            var userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(userDataClaim)
                ?? throw new InvalidOperationException("No se pudo leer la información del token.");

            var encrypt = new Encrypt(config["Jwt:KeyEncrypt"]!, config["Jwt:IvEncrypt"]!);
            var storedRefreshToken = encrypt.DecryptData(userData["refresh_token"].ToString()!);

            if (storedRefreshToken != request.Tokens.RefreshToken)
                return new Response<object>(TypeTransaction.ERR, "Refresh token inválido.");

            var idUsuario = Convert.ToInt32(userData["IdUsuario"]);
            var usuario = await repository.GetByIdAsync(idUsuario);
            if (usuario == null || usuario.IdEstado == "BLO")
                return new Response<object>(TypeTransaction.ERR, "Usuario no válido.");

            return new Response<object>(TypeTransaction.OK, BuildTokenPayload(usuario));
        }
        catch (Exception ex)
        {
            return new Response<object>(TypeTransaction.ERR, ex.Message);
        }
    }

    private object BuildTokenPayload(Usuario usuario)
    {
        var refreshToken = jwtHandler.GenerateRefreshToken();
        var encrypt = new Encrypt(config["Jwt:KeyEncrypt"]!, config["Jwt:IvEncrypt"]!);

        var userData = new Dictionary<string, object>
        {
            ["IdUsuario"] = usuario.IdUsuario,
            ["Username"] = usuario.Username,
            ["refresh_token"] = encrypt.EncryptData(refreshToken)
        };

        var token = jwtHandler.GenerateToken(usuario.Username, userData);
        var expires = config.GetValue<int>("Jwt:Expires");

        return new
        {
            isAuthenticated = true,
            accessToken = token,
            refresh_token = refreshToken,
            expiresIn = expires * 60,
            idUsuario = usuario.IdUsuario
        };
    }
}
