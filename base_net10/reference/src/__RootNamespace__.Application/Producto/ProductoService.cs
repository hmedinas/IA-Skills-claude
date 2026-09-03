using __RootNamespace__.Common.DTOs.Producto;

namespace __RootNamespace__.Application.Producto;

public record GetProductoByIdQuery(int Id) : IRequest<Response<ProductoDto>>;
public record GetProductosFilterQuery(int IdEmpresa, string? Estado = "ACT") : IRequest<Response<IEnumerable<ProductoDto>>>;
public record CrudProductoCommand(CrudProductoDto Dto) : IRequest<Transaction>;
public record AnularProductoCommand(int Id, int IdUsuario) : IRequest<Transaction>;

public class ProductoHandler(IProductoRepository repository, ILogger<ProductoHandler> logger) :
    IRequestHandler<GetProductoByIdQuery, Response<ProductoDto>>,
    IRequestHandler<GetProductosFilterQuery, Response<IEnumerable<ProductoDto>>>,
    IRequestHandler<CrudProductoCommand, Transaction>,
    IRequestHandler<AnularProductoCommand, Transaction>
{
    public async Task<Response<ProductoDto>> Handle(GetProductoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await repository.GetByIdAsync(request.Id);
            return data == null
                ? new Response<ProductoDto>(TypeTransaction.NOT_FOUND, "Producto no encontrado.")
                : new Response<ProductoDto>(TypeTransaction.OK, data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetProductoByIdQuery");
            return new Response<ProductoDto>(TypeTransaction.ERR, ex.Message);
        }
    }

    public async Task<Response<IEnumerable<ProductoDto>>> Handle(GetProductosFilterQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await repository.GetFilterAsync(request.IdEmpresa, request.Estado);
            return new Response<IEnumerable<ProductoDto>>(TypeTransaction.OK, data.Count(), data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetProductosFilterQuery");
            return new Response<IEnumerable<ProductoDto>>(TypeTransaction.ERR, ex.Message);
        }
    }

    public async Task<Transaction> Handle(CrudProductoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Dto.IdProducto <= 0)
            {
                var c = await repository.CreateAsync(request.Dto);
                return c.IsOk
                    ? new Transaction(TypeTransaction.OK, c.Mensaje, c.NuevoId.ToString())
                    : new Transaction(TypeTransaction.ERR, c.Mensaje, "DB_ERROR");
            }

            var u = await repository.UpdateAsync(request.Dto);
            return u.IsOk
                ? new Transaction(TypeTransaction.OK, u.Mensaje)
                : new Transaction(TypeTransaction.ERR, u.Mensaje, "DB_ERROR");
        }
        catch (Exception ex)
        {
            return new Transaction(TypeTransaction.ERR, ex.Message, "HANDLER_ERROR");
        }
    }

    public async Task<Transaction> Handle(AnularProductoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var r = await repository.AnularAsync(request.Id, request.IdUsuario);
            return r.IsOk
                ? new Transaction(TypeTransaction.OK, r.Mensaje)
                : new Transaction(TypeTransaction.ERR, r.Mensaje, "DB_ERROR");
        }
        catch (Exception ex)
        {
            return new Transaction(TypeTransaction.ERR, ex.Message, "HANDLER_ERROR");
        }
    }
}
