using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.UserDtos;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{

    DataContextDapper _dapper;
    public UserCompleteController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
    }
    [HttpGet ("TestConnection")]

    public DateTime TestConnection(){
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers/{usersId}/{isActive}")]

    // public IEnumerable<User> GetUsers()
    public IEnumerable<UserComplete> GetUsers(int usersId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string parameter = "";
        if (usersId != 0){
            parameter += ", @UserId= " + usersId.ToString();
        }
        if (isActive){
            parameter += ", @Active= " + isActive.ToString();
        }

        sql += parameter.Substring(1);

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        return users;
    }

    [HttpPut ("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user){
        string sql = @"EXEC TutorialAppSchema.spUser_Upsert
         @FirstName = '"+user.FirstName+
             "', @LastName  = '"+user.LastName+ 
             "', @Email = '"+user.Email+
             "', @Gender = '"+user.Gender+
             "', @Active = '" + user.Active +
             "', @JobTitle = '" + user.JobTitle +
             "', @Department= '" + user.Department +
             "', @Salary = '" + user.Salary +
             "', @UserId = " + user.UserId;

        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql)){
             return Ok();
        }else{
            throw new Exception("Falied to update user");
        }
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId){
        string sql = @"EXEC TutorialAppSchema.spUser_Delete @UserId = " + userId.ToString();
        if(_dapper.ExecuteSql(sql)){
            return Ok();
        }
        throw new Exception("Failed to Delete User");
    }

    // User Salary Section 
}