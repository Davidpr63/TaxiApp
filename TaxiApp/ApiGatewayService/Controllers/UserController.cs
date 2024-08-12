using ApiGatewayService.CloudStorageService;
using ApiGatewayService.DTOmodels;
using ApiGatewayService.Models;
using ApiGatewayService.QueueApiServiceCommunication;
using ApiGatewayService.QueueServiceCommunication;
using ApiGatewayService.TokenService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace ApiGatewayService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UpdateUserQueueService _updateUserQueueService;
        private readonly UpdateUserResponseQueue _updateUserResponseQueueService;
        private readonly DriversVerificationTableStorage _driversVerificationTableStorage;
        private readonly DriverApplicationQueueService _driverApplicationQueueService;
        private TokenGenerateService _tokenGenerateService;
        private TableStorageService _tableStorageService;
        private List<User> users = new List<User>();
        private List<DriverVerification> driverVerifications = new List<DriverVerification>();
        public UserController(DriverApplicationQueueService driverApplicationQueueService, DriversVerificationTableStorage driversVerificationTableStorage, UpdateUserQueueService updateUserQueueService, UpdateUserResponseQueue updateUserResponseQueue,TableStorageService tableStorageService, TokenGenerateService tokenGenerateService)
        {
            _tableStorageService = tableStorageService;
            _driversVerificationTableStorage = driversVerificationTableStorage;
            _updateUserQueueService = updateUserQueueService;
            _updateUserResponseQueueService = updateUserResponseQueue;
            _driverApplicationQueueService = driverApplicationQueueService;
            _tokenGenerateService = tokenGenerateService;
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUserData([FromForm] UserDTO updateUserData)
        {
            try
            {
                await LoadData("u");
                await _updateUserQueueService.QueueUpdateUserAsync(updateUserData);
                string respone = await _updateUserResponseQueueService.QueueUpdateUserResponseAsync();
                User LoggedInUser = users.FirstOrDefault(x => x.RowKey == updateUserData.UserId);
                if (respone.Equals("success"))
                {
                    var token = _tokenGenerateService.GenerateToken(LoggedInUser);
                    return Ok(new { success = true, token });
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

        [HttpPost("drivers-verification")]
        public async Task<IActionResult> DriverApplication([FromBody] UserIdDTO idDTO)
        {
            try
            {
               
                if (!string.IsNullOrEmpty(idDTO.UserId))
                {
                    await _driverApplicationQueueService.QueueDriverApplicationAsync(idDTO.UserId); 
                }
                else
                    return Ok(new { success = false, error = "id is null" });
                string respone = await _driverApplicationQueueService.QueueDriversVerificationResponseAsync();
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
        [HttpGet("get-verifications")]
        public async Task<IActionResult> AllVerifications()
        {
            try
            {
                await LoadData("v");
                return Ok(new { driverVerifications});
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
        }
     
        [HttpPost("approve-driver")]
        public async Task<IActionResult> ApproveDriver([FromBody] UserIdDTO idDTO)
        {
            if (!string.IsNullOrEmpty(idDTO.UserId))
            {
                await _driverApplicationQueueService.QueueApproveDriverAsync(idDTO.UserId);
            }
            else
                return Ok(new { success = false, error = "id is null" });
            string respone = await _driverApplicationQueueService.QueueApproveDriverResponseAsync();
            if (respone.Equals("success"))
            {
                return Ok(new { success = true });
            }
            else
            {
                return Ok(new { success = false, error = respone });
            }
        }

        [HttpPost("reject-drivers-request")]
        public async Task<IActionResult> RejectRequest([FromBody] UserIdDTO idDTO)
        {
            if (!string.IsNullOrEmpty(idDTO.UserId))
            {
                await _driverApplicationQueueService.QueueRejectRequestAsync(idDTO.UserId);
            }
            else
                return Ok(new { success = false, error = "id is null" });
            string respone = await _driverApplicationQueueService.QueueRejectRequestResponseAsync();
            if (respone.Equals("success"))
            {
                return Ok(new { success = true });
            }
            else
            {
                return Ok(new { success = false, error = respone });
            }
        }
        public async Task LoadData(string option)
        {
            try
            {
                switch (option)
                {
                    case "u": users = await _tableStorageService.RetrieveAllUsersAsync(); 
                        break;
                    case "v": driverVerifications = await _driversVerificationTableStorage.RetrieveAllVerificationsAsync();
                        break;
                    default:
                        break;
                }
                
            }
            catch (Exception)
            {
                Console.WriteLine("LodaData error");

            }
        }
    }
}
