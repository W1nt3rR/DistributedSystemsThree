using System;
using System.Linq;
using ConferenceService.Data;
using ConferenceService.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MemberService.Data
{
    public static class PrepDb
    {
        public async static Task PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                await SeedData(
                    serviceScope.ServiceProvider.GetService<AppDbContext>(),
                    serviceScope.ServiceProvider.GetService<IConferenceRepository>(),
                    isProd
                );
            }
        }

        private async static Task SeedData(AppDbContext context, IConferenceRepository conferenceRepository, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("Attempting to apply migrations...");
                try
                {
                    context.Database.Migrate();
                    Console.WriteLine("Migrations applied successfully...");

                    //Console.WriteLine("Attempting to seed the database...");
                    //MemberRegisterDTO memberRegisterDTO = new MemberRegisterDTO()
                    //{
                    //    FirstName = "John",
                    //    LastName = "Doe",
                    //    Email = "john.doe@gmail.com",
                    //    Password = "Omer@123",
                    //    ConfirmPassword = "Omer@123"
                    //};

                    //try
                    //{
                    //    var newMember = await conferenceRepository.AddAsync(memberRegisterDTO);
                    //    Console.WriteLine("Database seeded successfully...");
                    //}
                    //catch (Exception error)
                    //{
                    //    Console.WriteLine($"Could not seed database: {error.Message}");
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not run migrations: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Not in Production mode so no migrations will be run...");
            }
        }
    }
}