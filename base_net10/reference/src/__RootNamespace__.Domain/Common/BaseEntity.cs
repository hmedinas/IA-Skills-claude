namespace __RootNamespace__.Domain.Common;

public abstract class BaseEntity
{
    [StringLength(5)]
    [Column("IdEstado")]
    public string? IdEstado { get; set; } = "ACT";

    [Required]
    [Column("UsuarioCreacion")]
    public int UsuarioCreacion { get; set; }

    [Column("FechaCreacion")]
    public DateTime? FechaCreacion { get; set; } = DateTime.Now;

    [Column("UsuarioModificacion")]
    public int? UsuarioModificacion { get; set; }

    [Column("FechaModificacion")]
    public DateTime? FechaModificacion { get; set; }
}
