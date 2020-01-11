using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data.Interfaces;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController: ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository datingRepository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig )
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account account = new Account(
              _cloudinaryConfig.Value.CloudName,
              _cloudinaryConfig.Value.ApiKey,
              _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(Guid id)
        {
            var photoFromRepository = await _datingRepository.GetPhoto(id);
            var photo = _mapper.Map<PhotoToReturnDto>(photoFromRepository);
            return Ok(photo);
        }

        public async Task<IActionResult> AddPhotoForUser(Guid userId, [FromForm]PhotoForCreationDto photoForCreation)
        {
            var userFromRepository = await ValidateUserExist(userId);

            if (userFromRepository == null)
                return Unauthorized();

            var file = photoForCreation.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreation.Url = uploadResult.Uri.ToString();
            photoForCreation.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreation);

            if (!userFromRepository.Photos.Any(p => p.IsMain))
                photo.IsMain = true;

            userFromRepository.Photos.Add(photo);

            if (await _datingRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoToReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Couldn't add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(Guid userId, Guid id)
        {

            var user = await ValidateUserExist(userId);

            if (user == null)
                return Unauthorized();

            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepository = await _datingRepository.GetPhoto(id);

            if (photoFromRepository.IsMain)
                return BadRequest("This is already the main photo");

            var currentMainPhoto = await _datingRepository.GetUserMainPhoto(userId);

            currentMainPhoto.IsMain = false;
            photoFromRepository.IsMain = true;

            if (await _datingRepository.SaveAll())
                return NoContent();

            return BadRequest("Could not set the photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(Guid userId, Guid id)
        {
            var user = await ValidateUserExist(userId);

            if (user == null)
                return Unauthorized();

            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepository = await _datingRepository.GetPhoto(id);

            if (photoFromRepository.IsMain)
                return BadRequest("You cannot delete the main photo.");

            if(photoFromRepository.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepository.PublicId);

                var resultDeletion = _cloudinary.Destroy(deleteParams);

                if (resultDeletion.Result.Equals("ok"))
                    _datingRepository.Delete(photoFromRepository);
            }
            else
                _datingRepository.Delete(photoFromRepository);

            if (await _datingRepository.SaveAll())
                return Ok();

            return BadRequest("Failed to Delete Photo");
        }

        private async Task<User> ValidateUserExist(Guid userId)
        {
            if (userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return null;

            return await _datingRepository.GetUser(userId);
        }
    }
}
