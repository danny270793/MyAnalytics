namespace Backend.Requests;

public class UpdateUserRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
