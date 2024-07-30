using AuthService.CloudStorageService;
using AuthService.DTOmodels;
using AuthService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Fabric.Description;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private List<User> allUsers = new List<User>();
        public AuthController(TableStorageService tableStorageService, BlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserDTO newUser)
        {
            try
            {
                await LoadData();
                
                string imageUrl = await _blobStorageService.UploadImageAsync(newUser.image);
            //    await _tableStorageService.DeleteUserAsync("UserTable", "1");
                int rowkey = 0;
                string hashPass = "";
                TypeOfUser typeOfUser = TypeOfUser.User;
                if (!ExistUser(newUser.Email))
                {
                    if (!newUser.Password.Equals(newUser.ConfirmPassword))
                    {
                        return Ok(new { success = false, error = "Passwords dont match!" });
                    }
                    if (allUsers.Count == 0)
                    {
                        rowkey = 1;
                    }
                    else
                        rowkey = allUsers.Count + 1;
                    if (newUser.Email.Equals("david.02.petrovic@gmail.com"))
                    {
                        typeOfUser = TypeOfUser.Admin;
                    }
                    hashPass = HashPassword(newUser.Password);
                    User user = new User
                    {
                        RowKey = rowkey.ToString(),
                        Firstname = newUser.Firstname,
                        Lastname = newUser.Lastname,
                        Username = newUser.Username,
                        Email = newUser.Email,
                        Password = hashPass,
                        DateOfBirth = newUser.DateOfBirth,
                        Address = newUser.Address,
                        ImageUrl = imageUrl,
                        TypeOfUser = typeOfUser
                    };
                    await _tableStorageService.CreateUserAsync(user);
                }
                else
                {
                    return Ok(new { success = false, error = "That email already exists, please use another one." });
                }

                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
        }
       
       
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTOUser user)
        {
            try
            {
                await LoadData(); 
             
                User loggedInUser = allUsers.FirstOrDefault(x => x.Email == user.Email && x.Password == HashPassword(user.Password));

                if (loggedInUser != null)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return Ok(new { success = false, error = "Invalid email or password, please try again" });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
        }
        [HttpGet("facebook/callback")]
        public async Task<IActionResult> FacebookCallback(string code)
        {
            var clientId = "461062726838179";
            var clientSecret = "8e3560c3af9d8da687aafadd29c3b8b9";
            var redirectUri = "http://localhost:3000/facebook/callback";

            var tokenResponse = await new HttpClient().GetAsync($"https://graph.facebook.com/v8.0/oauth/access_token?client_id={clientId}&redirect_uri={redirectUri}&client_secret={clientSecret}&code={code}");
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var accessToken = JObject.Parse(tokenResponseContent)["access_token"].ToString();

            var userResponse = await new HttpClient().GetAsync($"https://graph.facebook.com/me?access_token={accessToken}&fields=id,name,email,picture");
            var userResponseContent = await userResponse.Content.ReadAsStringAsync();
            var user = JObject.Parse(userResponseContent);
            

            // Ovde obradite korisničke informacije kako bi se korisnik prijavio u vašu aplikaciju
            // Na primer, kreirajte ili pronađite korisnika u vašoj bazi podataka

            return Ok(new { message = "Facebook login successful!", user });
        }
        public async Task LoadData()
        {
            try
            {
                allUsers = await _tableStorageService.RetrieveAllUsersAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("LodaData error");

            }
        }
        public string HashPassword(string password)
        {
            try
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {

                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));


                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        public bool ExistUser(string email)
        {
            bool result = false;

            foreach (var item in allUsers)
            {
                if (item.Email == email)
                {
                    result = true;
                    return result;
                }
            }
            return result;
        }
    }
}
