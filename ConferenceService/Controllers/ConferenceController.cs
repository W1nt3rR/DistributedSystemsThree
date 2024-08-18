using ConferenceService.Models;
using ConferenceService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferenceController : ControllerBase
    {
        private readonly IConferenceRepository conferenceRepository;

        public ConferenceController(IConferenceRepository conferenceRepository)
        {
            this.conferenceRepository = conferenceRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var members = await conferenceRepository.GetAllAsync();
            return Ok(members);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var member = await conferenceRepository.GetByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register()
        {
            return Ok("not implemented");
        }

        [HttpGet]
        [Route("test")]
        public IActionResult Test()
        {
            return Ok("Test confrence");
        }

    }
}
