namespace Backend.Responses;

public class LoginResponse
{
    public required UserResponse User { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required long ExpiresIn { get; set; }
    public required string TokenType { get; set; }
}
