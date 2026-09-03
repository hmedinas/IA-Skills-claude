using __RootNamespace__.Common.Utils;

namespace __RootNamespace__.Infrastructure.Persistence;

public class AuthRepository(DapperContext context) : IAuthRepository
{
    private const int MaxIntentos = 3;

    public async Task<(Usuario? Usuario, bool IsBlocked, string? Mensaje)> ValidateCredentialsAsync(string username, string password)
    {
        using var db = context.CreateConnection();

        const string sql = "SELECT * FROM Usuarios WHERE Username = @Username AND IdEstado <> 'ANU'";
        var usuario = await db.QueryFirstOrDefaultAsync<Usuario>(sql, new { Username = username });

        if (usuario == null)
            return (null, false, "Usuario o contraseña incorrectos.");

        if (usuario.IdEstado == "BLO")
            return (null, true, "Usuario bloqueado por intentos fallidos. Contacte al administrador.");

        if (!PasswordHasher.VerifyPassword(password, usuario.PasswordHash))
        {
            usuario.Intentos += 1;
            var nuevoEstado = usuario.Intentos >= MaxIntentos ? "BLO" : usuario.IdEstado;

            const string updateSql = "UPDATE Usuarios SET Intentos = @Intentos, IdEstado = @IdEstado WHERE IdUsuario = @IdUsuario";
            await db.ExecuteAsync(updateSql, new { usuario.Intentos, IdEstado = nuevoEstado, usuario.IdUsuario });

            return (null, nuevoEstado == "BLO", "Usuario o contraseña incorrectos.");
        }

        if (usuario.Intentos > 0)
        {
            const string resetSql = "UPDATE Usuarios SET Intentos = 0 WHERE IdUsuario = @IdUsuario";
            await db.ExecuteAsync(resetSql, new { usuario.IdUsuario });
        }

        return (usuario, false, null);
    }

    public async Task<Usuario?> GetByIdAsync(int idUsuario)
    {
        using var db = context.CreateConnection();
        return await db.GetAsync(new Usuario { IdUsuario = idUsuario });
    }
}
