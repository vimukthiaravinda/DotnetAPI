using DotnetAPI.Data;
using DotnetAPI.DtosModels;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        [HttpGet("Posts/{postId}/{UserId}/{searchParam}")]
        public IEnumerable<Post>GetPosts(int postId = 0, int UserId = 0, string searchParam = "None")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string parameters = "";
            if(postId != 0 )
            {
                parameters += ", @PostId=" + postId.ToString();
            }
            if(UserId != 0)
            {
                parameters += ", @UserId=" + UserId.ToString();
            }
            if(searchParam.ToLower() != "None" )
            {
                parameters += ", @SearchValue='" + searchParam +"'";
            }

            if(parameters.Length > 0)
            {
                sql += parameters.Substring(1);
            }    
            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = '"+ this.User.FindFirst("userId")?.Value + "'";

            return _dapper.LoadData<Post>(sql);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToAddDto)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Upsert 
                @UserId = '"+ this.User.FindFirst("UserId")?.Value +
                 "', @PostTitle = '" + postToAddDto.PostTitle +
                 "', @PostContent = '"+ postToAddDto.PostContent + "'";
      
            if (postToAddDto.PostId > 0)
            {
                    sql += "', @PostId  = " + postToAddDto.PostId;
            }

            if(_dapper.ExecuteSql(sql)){

                return Ok();
            }
            throw new Exception("Faild to create new Post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC TutorialAppSchema.spPost_Delete @PostId  = '"+postId.ToString()+ "', @UserId = '"+this.User.FindFirst("UserId")?.Value+"'";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Faild to delete Post!");
        }
    }
}