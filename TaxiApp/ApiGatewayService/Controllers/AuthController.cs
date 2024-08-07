using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using System;
using System.Threading.Tasks;
using AuthenticationService.IAuth;
using ApiGatewayService.DTOmodels;
using System.ServiceModel;
using Microsoft.AspNetCore.Authentication;
using ApiGatewayService.QueueServiceCommunication;
using ApiGatewayService.CloudStorageService;
using ApiGatewayService.Models;
using System.Text;
using System.Security.Cryptography;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using ApiGatewayService.TokenService;
using System.Net;

namespace ApiGatewayService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RegistrationQueueService _authQueueService;
        private readonly LoginQueueService _loginQueueService;
        private readonly LoginResponseQueue _loginResponseQueueService;
        private readonly UpdateUserQueueService _updateUserQueueService;
        private readonly UpdateUserResponseQueue _updateUserResponseQueueService;

        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private TokenGenerateService _tokenGenerateService;
        private List<User> allUsers = new List<User>();
        public AuthController(TokenGenerateService tokenGenerateService, UpdateUserQueueService updateUserQueueService, UpdateUserResponseQueue updateUserResponseQueue, RegistrationQueueService authQueueService, LoginQueueService loginQueueService, LoginResponseQueue loginResponseQueueService,  TableStorageService tableStorageService, BlobStorageService blobStorageService)
        {
            _authQueueService = authQueueService;
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _loginQueueService = loginQueueService;
            _loginResponseQueueService = loginResponseQueueService;
            _tokenGenerateService = tokenGenerateService;
            _updateUserQueueService = updateUserQueueService;
            _updateUserResponseQueueService = updateUserResponseQueue;
            
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserDTO userDTO)
        {
            try
            {
                await LoadData();

                string imageUrl = await _blobStorageService.UploadImageAsync(userDTO.Image);
                //await _tableStorageService.DeleteUserAsync("UserTable", "1");
                int rowkey = 0;
                string hashPass = "";
                // TypeOfUser typeOfUser = TypeOfUser.User;
                TypeOfUser typeOfUser = TypeOfUser.User;
                if (!ExistUser(userDTO.Email))
                {
                    if (!userDTO.Password.Equals(userDTO.ConfirmPassword))
                    {
                        return Ok(new { success = false, error = "Passwords dont match!" });
                    }
                    if (allUsers.Count == 0)
                    {
                        rowkey = 1;
                    }
                    else
                        rowkey = allUsers.Count + 1;
                    if (userDTO.Email.Equals("david.02.petrovic@gmail.com"))
                    {
                        typeOfUser = TypeOfUser.Admin;
                    }
                    hashPass = HashPassword(userDTO.Password);
                    User user = new User
                    {
                        RowKey = rowkey.ToString(),
                        Firstname = userDTO.Firstname,
                        Lastname = userDTO.Lastname,
                        Username = userDTO.Username,
                        Email = userDTO.Email,
                        Password = hashPass,
                        DateOfBirth = userDTO.DateOfBirth,
                        Address = userDTO.Address,
                        ImageUrl = imageUrl,
                        TypeOfUser = typeOfUser
                    };
                    await _authQueueService.QueueUserRegistrationAsync(user);
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
                await _loginQueueService.QueueLoginUserAsync(user);
                User LoggedInUser = new User();
             
                await LoadData();
                LoggedInUser = allUsers.FirstOrDefault(x => x.Email == user.Email);

                if (await _loginResponseQueueService.QueueLoginResponseAsync())
                {
                    
                    var token = _tokenGenerateService.GenerateToken(LoggedInUser);
                    return Ok(new { success = true , token});
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
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUserData([FromForm] UserDTO updateUserData)
        {
            try
            {
                
                await _updateUserQueueService.QueueUpdateUserAsync(updateUserData);
                string respone = await _updateUserResponseQueueService.QueueUpdateUserResponseAsync();
                if (respone.Equals("success"))
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return Ok(new { success = false, error = respone });
                }



               

                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
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
