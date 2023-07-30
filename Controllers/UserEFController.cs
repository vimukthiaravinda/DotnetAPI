using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.UserDtos;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    IUserRepository _userRepository;
    IMapper _mapper ;
    public UserEFController(IConfiguration configuration, IUserRepository userRepository)
    {
        _userRepository = userRepository;

        _mapper = new Mapper(new MapperConfiguration(cfg =>{
            cfg.CreateMap<UserToAddDto, User>();
            cfg.CreateMap<UserSalary, UserSalary>();
            cfg.CreateMap<UserJobInfo, UserJobInfo>();
        }));
    }
    [HttpGet("GetUsers/{testValue}")]
    public string[] GetUsers(string testValue)
    {
        return new string[] {"user1", "user2", testValue};
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
       
    }
    [HttpPut ("EditUser")]
    public IActionResult EditUser(User user)
    {
         User? userdb = _userRepository.GetSingleUser(user.UserId);
        if (userdb != null)
        {
            userdb.Active = user.Active;
            userdb.FirstName = user.FirstName;
            userdb.LastName = user.LastName;
            userdb.Email = user.Email;
            userdb.Gender = user.Gender;
            if (_userRepository.SaveChanges()){
                return Ok();
            }
                throw new Exception("Falied to update user");
        }

        throw new Exception("Falied to update user");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user){

            User userDb = _mapper.Map<User>(user);

            _userRepository.AddEntity<User>(userDb);

            if (_userRepository.SaveChanges()){
                return Ok();
            }
                throw new Exception("Falied to update user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId){
        User? userdb = _userRepository.GetSingleUser(userId);
        if (userdb != null){
            _userRepository.RemoveEntity<User>(userdb);
            if (_userRepository.SaveChanges()){
                return Ok();
            }
             throw new Exception("Failed to Delete User");
        }     
        throw new Exception("Failed to Delete User");
    }

    [HttpPost("UserSalary")]
    public IActionResult PostUserSalaryEf(UserSalary userSalaryInsert){
        _userRepository.AddEntity<UserSalary>(userSalaryInsert);
        if(_userRepository.SaveChanges()){
            return Ok();
        }
        throw new Exception("Failed to Add User Salary");
    }

}
