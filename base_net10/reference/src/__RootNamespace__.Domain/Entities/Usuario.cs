namespace __RootNamespace__.Domain.Entities;

[Table("Usuarios")]
public class Usuario : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("IdUsuario")]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(50)]
    [Column("Username")]
    public string Username { get; set; } = string.Empty;

    [StringLength(50)]
    [Column("Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50)]
    [Column("Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("ResetPassword")]
    public bool ResetPassword { get; set; } = false;

    [Column("Intentos")]
    public int Intentos { get; set; } = 0;
}
