using System.ComponentModel.DataAnnotations;

namespace __RootNamespace__.Common.DTOs.Producto;

public class CrudProductoDto
{
    public int IdProducto { get; set; }

    [Required]
    public int IdEmpresa { get; set; }

    [Required]
    [StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Required]
    public decimal Precio { get; set; }

    [Required]
    public int IdUsuario { get; set; }
}
