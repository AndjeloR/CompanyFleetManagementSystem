using Microsoft.AspNetCore.Identity;

namespace CompanyFleetManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? RoleName { get; set; }
    }
}
