namespace University.Web.Utils.Auth;

public sealed class RegisterPostModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? ReturnUrl { get; set; }
}
