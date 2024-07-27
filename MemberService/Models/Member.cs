using Microsoft.AspNetCore.Identity;

namespace MemberService.Models
{
    public class Member : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
