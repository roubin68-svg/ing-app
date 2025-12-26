namespace IngApp.Application.Features.Auth.DTO;

public class AuthResponse
{
    public string Token { get; set; } = "";
    public DateTime? Expiration { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
