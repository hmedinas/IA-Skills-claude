namespace __RootNamespace__.Domain.Auth;

public class UserLoginCommand
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenCommand
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
