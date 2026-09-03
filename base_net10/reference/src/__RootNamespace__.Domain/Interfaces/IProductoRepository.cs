using __RootNamespace__.Common.DTOs.Producto;

namespace __RootNamespace__.Domain.Interfaces;

public interface IProductoRepository
{
    Task<ProductoDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductoDto>> GetFilterAsync(int idEmpresa, string? estado = "ACT");
    Task<(int NuevoId, string Mensaje, bool IsOk)> CreateAsync(CrudProductoDto dto);
    Task<(bool IsOk, string Mensaje)> UpdateAsync(CrudProductoDto dto);
    Task<(bool IsOk, string Mensaje)> AnularAsync(int id, int idUsuario);
}
