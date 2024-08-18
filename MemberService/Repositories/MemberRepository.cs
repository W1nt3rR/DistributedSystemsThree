using MemberService.Data;
using MemberService.DTOs;
using MemberService.Models;
using MemberService.SyncDataService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MemberService.Repositories
{
    public interface IMemberRepository
    {
        Task<IEnumerable<Member>> GetAllAsync();
        Task<Member> GetByIdAsync(string id);
        Task<Member> AddAsync(MemberRegisterDTO member);
        Task<string> LoginAsync(MemberLoginDTO member);
        Task UpdateAsync(Member member);
        Task DeleteAsync(string id);
        Task<bool> Signup(string memberToken, string conferenceId);
    }

    public class MemberRepository : IMemberRepository
    {
        private readonly UserManager<Member> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext _context;
        private readonly IEventDataClient eventDataClient;
        private readonly IConfiguration config;

        public MemberRepository(
            UserManager<Member> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IEventDataClient eventDataClient,
            IConfiguration config
        )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _context = context;
            this.eventDataClient = eventDataClient;
            this.config = config;
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
                LastName = member.LastName,
            };

            // check if already exuists

            var user = await userManager.FindByEmailAsync(member.Email);

            if (user != null)
            {
                throw new Exception("User already exists");
            }

            var newUser = await userManager.CreateAsync(newMember, member.Password);
            if (!newUser.Succeeded)
            {
                throw new Exception("Failed to create user");
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

        public async Task<string> LoginAsync(MemberLoginDTO member)
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

            var userRoles = await userManager.GetRolesAsync(user);
            var userSession = new MemberSession(user.Email, userRoles.ToList());
            string token = GenerateToken(userSession);

            return token;
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

        private string GenerateToken(MemberSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email)
        };
            userClaims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var audiences = config.GetSection("Jwt:Audiences").Get<List<string>>();
            userClaims.AddRange(audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)));

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> Signup(string memberToken, string conferenceId)
        {
            return await eventDataClient.Signup(memberToken, conferenceId);
        }
    }

    public record MemberSession(string Email, List<string> Roles);
}
