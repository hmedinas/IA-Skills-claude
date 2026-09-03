namespace __RootNamespace__.Domain.Entities;

[Table("Producto")]
public class Producto : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("IdProducto")]
    public int IdProducto { get; set; }

    [Required]
    [Column("IdEmpresa")]
    public int IdEmpresa { get; set; }

    [Required]
    [StringLength(30)]
    [Column("Codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Column("Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Column("Descripcion")]
    public string? Descripcion { get; set; }

    [Required]
    [Column("Precio")]
    public decimal Precio { get; set; }
}
