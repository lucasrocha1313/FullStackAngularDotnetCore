using System.Threading.Tasks;
using DatingApp.API.Data.Interfaces;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string userName, string password)
        {
            //validate request
            userName = userName.ToLower();

            if(await _authRepository.UserExists(userName))
            {
                return BadRequest("Username already Exists!");
            }

            var userToCreate = new User
            {
                Name = userName
            };

            var createdUser = await _authRepository.Register(userToCreate, password);

            return StatusCode(201);
        }
    }
}