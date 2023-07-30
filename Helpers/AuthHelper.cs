using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.DtosModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly DataContextDapper  _dapper;
        private readonly IConfiguration _config;
        public AuthHelper(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }
        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltpulsString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);
            return KeyDerivation.Pbkdf2(password: password,salt:Encoding.ASCII.GetBytes(passwordSaltpulsString),prf:KeyDerivationPrf.HMACSHA512,
                    iterationCount:1000000,numBytesRequested: 256 / 8);
        }
        public string CreateToken(int userId)
        {
            Claim[] claims = new Claim[]{
                new Claim("userId", userId.ToString())
            };

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:TokenKey").Value));
            
            SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor(){
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool setPassword(UserForLoginDto userForPassword)
        {
                    byte[] passwordSalt = new byte[128 /8];
                    using(RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }
                    byte[] passwordHash = GetPasswordHash(userForPassword.Password,passwordSalt);
                    string sqlAddAuth = "EXEC TutorialAppSchema.spRegistration_Upsert @Email = @EmailParam, @PasswordHash = @PasswordHashParam, @PasswordSalt = @PasswordSaltParam";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter emailParameter = new SqlParameter("@EmailParam", System.Data.SqlDbType.VarChar);
                    emailParameter.Value = userForPassword.Email;
                    sqlParameters.Add(emailParameter);

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParam", System.Data.SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;
                    sqlParameters.Add(passwordSaltParameter);

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", System.Data.SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;
                    sqlParameters.Add(passwordHashParameter);

                    return(_dapper.ExecutesqlwithParameters(sqlAddAuth, sqlParameters));
        }
    }
}