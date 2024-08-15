using ConferenceService.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceService.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Conference> Conferences { get; set; }
    }
}
