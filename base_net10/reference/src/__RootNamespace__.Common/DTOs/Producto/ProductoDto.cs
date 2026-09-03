namespace __RootNamespace__.Common.DTOs.Producto;

public class ProductoDto
{
    public int IdProducto { get; set; }
    public int IdEmpresa { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string IdEstado { get; set; } = "ACT";
    public int UsuarioCreacion { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
}
