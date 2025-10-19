namespace MetalSavingsManager.Containers;
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class UpdatePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public Guid UserId { get; set; }
}