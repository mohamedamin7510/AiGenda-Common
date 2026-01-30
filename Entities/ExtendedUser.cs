using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class ExtendedUser:IdentityUser
{
    public string? FirstName { get; set; }
    public string? SecondName { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new(); 
}
