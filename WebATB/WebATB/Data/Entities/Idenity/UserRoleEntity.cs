using Microsoft.AspNetCore.Identity;
using WebATB.Data.Entities.Identity;

namespace WebATB.Data.Entities.Idenity;

public class UserRoleEntity : IdentityUserRole<int>
{
    public UserEntity User { get; set; } = null!;
    public RoleEntity Role { get; set; } = null!;
}
