using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.DtosModels;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper  _dapper;
        private readonly AuthHelper _authHelper;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistrationDto)
        {
            if (userForRegistrationDto.Password.Equals(userForRegistrationDto.PasswordConfirm))
            {
                string sqlCheckUserExists = @"SELECT Email from TutorialAppSchema.Auth WHERE Email = '" +userForRegistrationDto.Email+"'";
                IEnumerable<string> existeUser = _dapper.LoadData<string>(sqlCheckUserExists);
                if(existeUser.Count().Equals(0))
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto(){
                        Email = userForRegistrationDto.Email, 
                        Password = userForRegistrationDto.Password
                    };
                    if(_authHelper.setPassword(userForSetPassword))
                    {
                        string sqlAddUser = @"EXEC TutorialAppSchema.spUser_Upsert
                                            @FirstName = '"+ userForRegistrationDto.FirstName+
                                            "', @LastName  = '"+ userForRegistrationDto.LastName+ 
                                            "', @Email = '"+ userForRegistrationDto.Email+
                                            "', @Gender = '"+ userForRegistrationDto.Gender+
                                            "', @Active = 1 " +
                                            ", @JobTitle = '" + userForRegistrationDto.JobTitle +
                                            "', @Department= '" + userForRegistrationDto.Department +
                                            "', @Salary = '" + userForRegistrationDto.Salary +"'";
                    // string sqlAddUser = @"
                    // INSERT INTO TutorialAppSchema.Users(
                    //         [FirstName],
                    //         [LastName],
                    //         [Email],
                    //         [Gender],
                    //         [Active]
                    //     ) VALUES ("+
                    //     "'" + userForRegistrationDto.FirstName +
                    //     "','"+userForRegistrationDto.LastName+ 
                    //     "','"+userForRegistrationDto.Email+
                    //     "','"+userForRegistrationDto.Gender+
                    //     "', 1 )";
                        if (_dapper.ExecuteSql(sqlAddUser)){
                             return Ok();
                        }
                        throw new Exception ("Faild to add User");
                    }
                    throw new Exception("Faild to register user..!");
                }
                throw new Exception("User with this email already exists..");
            }
            throw new Exception("Password do not match");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword)
        {
            if (_authHelper.setPassword(userForSetPassword))
            {
                return Ok();
            }
            throw new Exception("Faild to Update Password!");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            string sqlForHashAndSalt = @"SELECT
                    [PasswordHash],
                    [PasswordSalt] from TutorialAppSchema.Auth WHERE Email = '" +userForLoginDto.Email+ "'";
            UserForLoginConfirmationDto userForLoginConfirmation = _dapper.LoadDataSingle<
            UserForLoginConfirmationDto>(sqlForHashAndSalt);
            byte[] passwordHash = _authHelper.GetPasswordHash(userForLoginDto.Password, userForLoginConfirmation.PasswordSalt);
            for(int index = 0; index< passwordHash.Length; index++){
                if(passwordHash[index] != userForLoginConfirmation.PasswordHash[index]){
                    return StatusCode(401,"Incorrect password!");
                }
            }
            string userIdSql = @"SELECT [UserId] FROM TutorialAppSchema.Users WHERE Email = '" + userForLoginDto.Email +"'";
            int userId = _dapper.LoadDataSingle<int>(userIdSql);
            return Ok(new Dictionary<string, string>{
                {"token", _authHelper.CreateToken(userId)}
            });
        }
        [HttpGet("RefershToken")]
        public string RefershToken()
        {
            string sqlGetUserId = @"SELECT [UserId] FROM TutorialAppSchema.Users WHERE UserId = '" + User.FindFirst("UserId")?.Value +"'";
            int userId = _dapper.LoadDataSingle<int>(sqlGetUserId);
            return _authHelper.CreateToken(userId);
        }
    }
}