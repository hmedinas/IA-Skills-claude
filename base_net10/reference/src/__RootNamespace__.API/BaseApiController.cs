using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;

namespace __RootNamespace__.API;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
/*[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]*/
[Route("[controller]")]
public abstract class BaseApiController(ILogger logger) : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
    protected ILogger Logger => logger;

    protected ActionResult HandleResponse<T>(Response<T> result) where T : class
    {
        if (result == null) return BadRequest(new { Message = "Respuesta nula" });

        return result.Type switch
        {
            TypeTransaction.OK => Ok(result),
            TypeTransaction.NOT_FOUND => NotFound(result),
            TypeTransaction.ERR => Ok(result),
            _ => StatusCode(500, "Unhandled transaction type.")
        };
    }

    protected ActionResult HandleTransaction(Transaction transaction)
    {
        if (transaction == null) return BadRequest(new { Message = "Transacción nula" });

        return transaction.Type switch
        {
            TypeTransaction.OK => Ok(transaction),
            TypeTransaction.NOT_FOUND => NotFound(transaction),
            TypeTransaction.ERR => Ok(transaction),
            _ => StatusCode(500, "Unhandled transaction type.")
        };
    }
}
