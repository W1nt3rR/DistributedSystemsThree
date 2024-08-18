using ConferenceService.Data;
using ConferenceService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConferenceService.Repositories
{
    public interface IConferenceRepository
    {
        Task<IEnumerable<Conference>> GetAllAsync();
        Task<Conference> GetByIdAsync(int id);
        Task UpdateAsync(Conference member);
        Task DeleteAsync(string id);
    }

    public class ConferenceRepository : IConferenceRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration config;

        public ConferenceRepository(
            AppDbContext context,
            IConfiguration config
        )
        {
            _context = context;
            this.config = config;
        }

        public async Task<IEnumerable<Conference>> GetAllAsync()
        {
            return await _context.Conferences.ToListAsync();
        }

        public async Task<Conference> GetByIdAsync(int id)
        {
            return await _context.Conferences.FindAsync(id);
        }

        public async Task UpdateAsync(Conference member)
        {
            _context.Conferences.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var member = await _context.Conferences.FindAsync(id);
            if (member != null)
            {
                _context.Conferences.Remove(member);
                await _context.SaveChangesAsync();
            }
        }
    }
}
