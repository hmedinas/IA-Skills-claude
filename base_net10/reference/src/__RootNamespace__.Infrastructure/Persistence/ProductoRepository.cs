using __RootNamespace__.Common.DTOs.Producto;

namespace __RootNamespace__.Infrastructure.Persistence;

public class ProductoRepository(DapperContext context) : IProductoRepository
{
    public async Task<ProductoDto?> GetByIdAsync(int id)
    {
        using var db = context.CreateConnection();
        var entity = await db.GetAsync(new Producto { IdProducto = id });
        return entity == null ? null : Map(entity);
    }

    public async Task<IEnumerable<ProductoDto>> GetFilterAsync(int idEmpresa, string? estado)
    {
        using var db = context.CreateConnection();
        const string sql = @"
            SELECT IdProducto, IdEmpresa, Codigo, Nombre, Descripcion, Precio,
                   IdEstado, UsuarioCreacion,
                   FORMAT(FechaCreacion, 'dd/MM/yyyy HH:mm') AS FechaCreacion
            FROM Producto
            WHERE IdEmpresa = @IdEmpresa
              AND (@Estado IS NULL OR @Estado = '' OR IdEstado = @Estado)
            ORDER BY IdProducto DESC";

        return await db.QueryAsync<ProductoDto>(sql, new { IdEmpresa = idEmpresa, Estado = estado });
    }

    public async Task<(int NuevoId, string Mensaje, bool IsOk)> CreateAsync(CrudProductoDto dto)
    {
        using var db = context.CreateConnection();
        var entity = new Producto
        {
            IdEmpresa = dto.IdEmpresa,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            IdEstado = "ACT",
            UsuarioCreacion = dto.IdUsuario,
            FechaCreacion = DateTime.Now
        };

        await db.InsertAsync(entity);
        return (entity.IdProducto, "Producto creado correctamente.", true);
    }

    public async Task<(bool IsOk, string Mensaje)> UpdateAsync(CrudProductoDto dto)
    {
        using var db = context.CreateConnection();
        var entity = await db.GetAsync(new Producto { IdProducto = dto.IdProducto });
        if (entity == null) return (false, "Producto no encontrado.");

        entity.Codigo = dto.Codigo;
        entity.Nombre = dto.Nombre;
        entity.Descripcion = dto.Descripcion;
        entity.Precio = dto.Precio;
        entity.UsuarioModificacion = dto.IdUsuario;
        entity.FechaModificacion = DateTime.Now;

        await db.UpdateAsync(entity);
        return (true, "Producto actualizado correctamente.");
    }

    public async Task<(bool IsOk, string Mensaje)> AnularAsync(int id, int idUsuario)
    {
        using var db = context.CreateConnection();
        var entity = await db.GetAsync(new Producto { IdProducto = id });
        if (entity == null) return (false, "Producto no encontrado.");

        entity.IdEstado = "ANU";
        entity.UsuarioModificacion = idUsuario;
        entity.FechaModificacion = DateTime.Now;

        await db.UpdateAsync(entity);
        return (true, "Producto anulado.");
    }

    private static ProductoDto Map(Producto e) => new()
    {
        IdProducto = e.IdProducto,
        IdEmpresa = e.IdEmpresa,
        Codigo = e.Codigo,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        Precio = e.Precio,
        IdEstado = e.IdEstado ?? "ACT",
        UsuarioCreacion = e.UsuarioCreacion,
        FechaCreacion = e.FechaCreacion?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty
    };
}
