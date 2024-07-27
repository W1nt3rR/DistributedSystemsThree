using MemberService.Data;
using MemberService.DTOs;
using MemberService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Repositories
{
    public interface IMemberRepository
    {
        Task<IEnumerable<Member>> GetAllAsync();
        Task<Member> GetByIdAsync(string id);
        Task<Member> AddAsync(MemberRegisterDTO member);
        Task<Member> LoginAsync(MemberLoginDTO member);
        Task UpdateAsync(Member member);
        Task DeleteAsync(string id);
    }

    public class MemberRepository : IMemberRepository
    {
        private readonly UserManager<Member> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext _context;

        public MemberRepository(
            UserManager<Member> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context
        )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _context = context;
        }

        public async Task<IEnumerable<Member>> GetAllAsync()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<Member> GetByIdAsync(string id)
        {
            return await _context.Members.FindAsync(id);
        }

        public async Task<Member> AddAsync(MemberRegisterDTO member)
        {
            var newMember = new Member
            {
                UserName = member.Email,
                Email = member.Email,
                FirstName = member.FirstName,
                LastName = member.LastName
            };

            // check if already exuists
            var user = await userManager.FindByEmailAsync(member.Email);

            if (user != null)
            {
                return null;
            }

            var newUser = await userManager.CreateAsync(newMember, member.Password);
            if (!newUser.Succeeded)
            {
                return null;
            }

            var roleCheck = await roleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                await roleManager.CreateAsync(new IdentityRole("Member"));

                await userManager.AddToRoleAsync(newMember, "Admin");
            }

            await userManager.AddToRoleAsync(newMember, "Member");

            return newMember;
        }

        public async Task<Member> LoginAsync(MemberLoginDTO member)
        {
            var user = await userManager.FindByEmailAsync(member.Email);
            if (user == null) {
                return null;
            }

            bool isPasswordValid = await userManager.CheckPasswordAsync(user, member.Password);
            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }

        public async Task UpdateAsync(Member member)
        {
            _context.Members.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
        }
    }
}
