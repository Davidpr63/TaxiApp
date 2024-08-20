
using ApiGatewayService.Models;
using ApiGatewayService.TokenService;
using Facebook;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiGatewayService.FacebookAuth
{
    public class FacebookAuthService : IFacebookAuthService
    {
        private readonly string _appId = "461062726838179";
        private readonly string _appSecret = "8e3560c3af9d8da687aafadd29c3b8b9";
        private readonly string? secretKey = "a3f7b1c6e4d8a9e0c5f2d6b8a1e9d3c4f7b1e0a5c9d8f6b2e1a3c4d8e0f5b6a7";
        private readonly string? issuer = "http://localhost:8874/";
        private readonly string? audience = "http://localhost:3000/";

         
        public async Task<FacebookAuthResult> AuthenticateAsync(string accessToken, int numOfUser)
        {
            var fb = new FacebookClient(accessToken);
            dynamic userInfo = await fb.GetTaskAsync("me?fields=id,name,email,picture");

            if (userInfo == null)
            {
                return new FacebookAuthResult { Success = false, Errors = new[] { "Invalid Facebook token." } };
            }
            User user = new User()
            {
                RowKey = (numOfUser + 1).ToString(),
                Firstname = userInfo.name,
                Email = userInfo.email,
                TypeOfUser = TypeOfUser.User
            }; 
            

            return new FacebookAuthResult
            {
                User = user,
                Success = true,
                Token = GenerateJwtToken(user)  
            };
        }

        private string GenerateJwtToken(User user)
        {
           
            string token = GenerateToken(user);
            return token;
        }
        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userJson = JsonConvert.SerializeObject(user);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userJson),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
