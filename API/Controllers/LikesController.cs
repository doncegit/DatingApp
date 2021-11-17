using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{   [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        public LikesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var SourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var SourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(SourceUserId);

            if(likedUser == null) return NotFound();

            if(SourceUser.UserName ==username) return BadRequest("you cannot like yourself");

            var userLike = await _unitOfWork.LikesRepository.GetUserLike(SourceUserId, likedUser.Id);

            if(userLike != null) return BadRequest("You already like this user");

            userLike = new Entities.UserLike 
            {
                SourceUserId = SourceUserId,
                LikedUserId = likedUser.Id
            };
            SourceUser.LikedUsers.Add(userLike);

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes ([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, 
            users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }
    }
}