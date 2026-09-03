using __RootNamespace__.Application.Producto;
using __RootNamespace__.Common.DTOs.Producto;

namespace __RootNamespace__.API.Controllers;

[SwaggerTag("Módulo de ejemplo: CRUD de Productos")]
public class ProductosController(ILogger<ProductosController> logger) : BaseApiController(logger)
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => HandleResponse(await Mediator.Send(new GetProductoByIdQuery(id)));

    [HttpGet]
    public async Task<IActionResult> GetFilter([FromQuery] int idEmpresa, [FromQuery] string? estado = "ACT")
        => HandleResponse(await Mediator.Send(new GetProductosFilterQuery(idEmpresa, estado)));

    [HttpPost]
    public async Task<ActionResult<Transaction>> Crud([FromBody] CrudProductoDto dto)
        => HandleTransaction(await Mediator.Send(new CrudProductoCommand(dto)));

    [HttpPut("{id}/anular")]
    public async Task<ActionResult<Transaction>> Anular(int id, [FromQuery] int idUsuario)
        => HandleTransaction(await Mediator.Send(new AnularProductoCommand(id, idUsuario)));
}
