﻿using AutoMapper;
using DatingApp.API.Data.Interfaces;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController: ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository datingRepository, IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<ActionResult> GetMessage(Guid userId, Guid id)
        {
            if (checkUserAuthorized(userId))
                return Unauthorized();

            var messageFromRepository = _datingRepository.GetMessage(id);

            if (messageFromRepository == null)
                return NotFound();

            return Ok(messageFromRepository);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(Guid userId, [FromQuery]MessageParams messageParams)
        {
            if (checkUserAuthorized(userId))
                return Unauthorized();

            messageParams.UserId = userId;

            var messagesFromRepository = await _datingRepository.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepository);

            Response.AddPagination(messagesFromRepository.CurrentPage, messagesFromRepository.PageSize, 
                messagesFromRepository.TotalCount, messagesFromRepository.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(Guid userId, Guid recipientId)
        {
            if (checkUserAuthorized(userId))
                return Unauthorized();

            var messagesFromRepository = await _datingRepository.GetMessageThread(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepository);

            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(Guid userId, MessageForCreationDto messageForCreationDto)
        {
            if (checkUserAuthorized(userId))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;

            var recipient = await _datingRepository.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
                BadRequest("Could not find user");

            var message = _mapper.Map<Message>(messageForCreationDto);

            _datingRepository.Add(message);

            var messageToReturn = _mapper.Map<MessageForCreationDto>(message);

            if (await _datingRepository.SaveAll())
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);

            throw new Exception("Creating message failed on save");
        }

        private bool checkUserAuthorized(Guid userId)
        {
            return userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
    }
}
