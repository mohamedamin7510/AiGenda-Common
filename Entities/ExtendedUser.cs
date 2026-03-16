using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_genda_API.Entities;

public class ExtendedUser:IdentityUser
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string SubscriptionType { get; set; } = "Free";
    public string? JobTitle { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; } = default;
    public bool IsDeleted { get; set; } = false;

    public List<WorkspaceMember> WorkSpaceMembers { get; set; } = [];
    public List<RefreshToken> RefreshTokens { get; set; } = []; 

 
}
