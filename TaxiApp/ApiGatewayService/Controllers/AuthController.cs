using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using System;
using System.Threading.Tasks;
using AuthenticationService.IAuth;
using ApiGatewayService.DTOmodels;
using System.ServiceModel;
using Microsoft.AspNetCore.Authentication;
using ApiGatewayService.QueueApiServiceCommunication;
using ApiGatewayService.CloudStorageService;
using ApiGatewayService.Models;
using System.Text;
using System.Security.Cryptography;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using ApiGatewayService.TokenService;
using System.Net;
using Azure;
using ApiGatewayService.DtoMapper;
using ApiGatewayService.DtoMapper.IDtoMapper;
using ApiGatewayService.FacebookAuth;

namespace ApiGatewayService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RegistrationQueueService _authQueueService;
        private readonly LoginQueueService _loginQueueService;
        
       

        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private TokenGenerateService _tokenGenerateService;
        private List<User> allUsers = new List<User>();
        private readonly IMapper _mapper;
        private readonly IFacebookAuthService _facebookAuthService;

        public AuthController(IMapper mapper, IFacebookAuthService facebookAuthService, TokenGenerateService tokenGenerateService, RegistrationQueueService authQueueService, LoginQueueService loginQueueService, TableStorageService tableStorageService, BlobStorageService blobStorageService)
        {
            _authQueueService = authQueueService;
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _loginQueueService = loginQueueService;
            _tokenGenerateService = tokenGenerateService;
            _mapper = mapper;
            _facebookAuthService = facebookAuthService;
            
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserDTO userDTO)
        {
            try
            {
               
                userDTO.ImageUrl = await _blobStorageService.UploadImageAsync(userDTO.Image);             
                User user = _mapper.DtoUserToUser(userDTO);             
                await _authQueueService.QueueUserRegistrationAsync(user);
                string response = await _authQueueService.QueueRegistrationUserResponseAsync();
                if (response.Equals("success"))
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return Ok(new { success = false, error = response });
                }
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
                string response = await _loginQueueService.QueueLoginResponseAsync();
                if (response.Equals("success"))
                {

                    User LoggedInUser = new User();
                    await LoadData();
                    LoggedInUser = allUsers.FirstOrDefault(x => x.Email == user.Email);
                    var token = _tokenGenerateService.GenerateToken(LoggedInUser);
                    return Ok(new { success = true , token});
                }
                else
                {
                    return Ok(new { success = false, error = response });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
        }

        [HttpPost("facebook-login")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookAuthRequestDTO request)
        {
            
            await LoadData();
            var authResult = await _facebookAuthService.AuthenticateAsync(request.AccessToken, allUsers.Count);
            await _tableStorageService.CreateUserAsync(authResult.User);
            if (!authResult.Success)
            {
                return BadRequest(authResult.Errors);
            }
            string token = authResult.Token;
            
            return Ok(new { token });
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
      
    }
  
}
