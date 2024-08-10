using MemberService.DTOs;
using MemberService.Models;
using MemberService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MemberService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberRepository memberRepository;

        public MemberController(IMemberRepository memberRepository)
        {
            this.memberRepository = memberRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var members = await memberRepository.GetAllAsync();
            return Ok(members);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var member = await memberRepository.GetByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(MemberRegisterDTO member)
        {
            try
            {
                var newMember = await memberRepository.AddAsync(member);
                return Ok(newMember);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(MemberLoginDTO member)
        {
            var user = await memberRepository.LoginAsync(member);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }

        [HttpGet]
        [Route("test")]
        //[Authorize]
        public IActionResult Test()
        {
            return Ok("Test");
        }

    }
}
