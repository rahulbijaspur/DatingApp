using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRespository _likesRespository;
        public LikesController(IUserRepository userRepository, ILikesRespository likesRespository)
        {
            _likesRespository = likesRespository;
            _userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId =User.GetUserId();
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            var SourceUser = await _likesRespository.GetUserWithLikes(sourceUserId);

            if (likedUser ==null) return NotFound();
            if (SourceUser.UserName == username ) return BadRequest("You cannout like yourself");
            var UserLike = await _likesRespository.GetUserLike(sourceUserId,likedUser.Id);
            if (UserLike != null) return BadRequest("You already liked this user");
            UserLike = new UserLike
            {
                SourceUserId =sourceUserId,
                LikedUserId = likedUser.Id
            };
            SourceUser.LikedUsers.Add(UserLike);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
            
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId =User.GetUserId();
            var users= await _likesRespository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);        
        }
    }
}