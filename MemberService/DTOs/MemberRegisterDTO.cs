using System.ComponentModel.DataAnnotations;

namespace MemberService.DTOs
{
    public class MemberRegisterDTO
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
