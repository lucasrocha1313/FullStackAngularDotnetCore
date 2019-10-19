using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data.Interfaces;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository datingRepository, IMapper mapper)
        {
            _mapper = mapper;
            _datingRepository = datingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParam userParam)
        {
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            userParam.UserId = currentUserId;

            var currentUser = await _datingRepository.GetUser(currentUserId);

            if(string.IsNullOrEmpty(userParam.Gender))
            {
                userParam.Gender = currentUser.Gender.ToLower().Equals("male") ? "female" : "male";
            }

            var users = await _datingRepository.GetUsers(userParam);

            var usersForReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersForReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _datingRepository.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserForUpdateDto userForUpdate)
        {
            if (id != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepository = await _datingRepository.GetUser(id);

            _mapper.Map(userForUpdate, userFromRepository);

            if (await _datingRepository.SaveAll())
                return NoContent();

            throw new Exception($"updating user {id} failed on save!");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(Guid id, Guid recipientId)
        {
            if (id != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await _datingRepository.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("You already liked this user.");

            if (await _datingRepository.GetUser(recipientId) == null)
                return NotFound();

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _datingRepository.Add(like);

            if (await _datingRepository.SaveAll())
                return Ok();
            else
                return BadRequest("Failed to like user");
        }

    }
}