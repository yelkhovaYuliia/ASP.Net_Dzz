using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using WebATB.Data.Entities.Idenity;

namespace WebATB.Data.Entities.Identity
{
    public class UserEntity : IdentityUser<int>
    {
        [StringLength(100)]
        public string? FirstName { get; set; } = null;
        [StringLength(100)]
        public string? LastName { get; set; } = null;
        [StringLength(100)]
        public string? Image { get; set; } = null;

        public ICollection<UserRoleEntity> UserRoles { get; set; }

        public ICollection<IdentityUserLogin<int>> Logins { get; set; } = new List<IdentityUserLogin<int>>();
    }
}