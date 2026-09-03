using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace __RootNamespace__.Host.Extensions;

public class ApiPrefixConvention : IControllerModelConvention
{
    private readonly string _prefix;

    public ApiPrefixConvention(string prefix)
    {
        _prefix = prefix;
    }

    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            var prefixRoute = new AttributeRouteModel(new RouteAttribute(_prefix));

            if (selector.AttributeRouteModel != null)
            {
                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                    prefixRoute,
                    selector.AttributeRouteModel);
            }
            else
            {
                selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute($"{_prefix}/[controller]"));
            }
        }
    }
}
