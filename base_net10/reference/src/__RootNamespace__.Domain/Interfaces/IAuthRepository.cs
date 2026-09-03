using __RootNamespace__.Domain.Entities;

namespace __RootNamespace__.Domain.Interfaces;

public interface IAuthRepository
{
    Task<(Usuario? Usuario, bool IsBlocked, string? Mensaje)> ValidateCredentialsAsync(string username, string password);
    Task<Usuario?> GetByIdAsync(int idUsuario);
}
