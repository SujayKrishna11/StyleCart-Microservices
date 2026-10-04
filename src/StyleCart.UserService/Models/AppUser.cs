using Microsoft.AspNetCore.Identity;

namespace StyleCart.UserService.Models;

public sealed class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}