using MemberService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Data
{
    public class AppDbContext(DbContextOptions options) : IdentityDbContext<Member>(options)
    {
        public DbSet<Member> Members { get; set; }
    }
}
