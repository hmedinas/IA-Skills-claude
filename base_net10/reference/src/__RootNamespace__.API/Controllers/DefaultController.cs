using Microsoft.Extensions.Hosting;

namespace __RootNamespace__.API.Controllers;

[Route("/")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DefaultController(IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
        => env.IsDevelopment() ? Redirect("/swagger") : Redirect("/health");
}
