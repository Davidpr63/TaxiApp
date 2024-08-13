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
        private readonly RideQueueService _rideQueueService;
        private TokenGenerateService _tokenGenerateService;
        private TableStorageService _tableStorageService;
        private RidesTableStorage _ridesTableStorage;
        private List<User> users = new List<User>();
        private List<DriverVerification> driverVerifications = new List<DriverVerification>();
        private List<Ride> allRides = new List<Ride>();
        public UserController(RidesTableStorage ridesTableStorage, RideQueueService rideQueueService, DriverApplicationQueueService driverApplicationQueueService, DriversVerificationTableStorage driversVerificationTableStorage, UpdateUserQueueService updateUserQueueService, UpdateUserResponseQueue updateUserResponseQueue,TableStorageService tableStorageService, TokenGenerateService tokenGenerateService)
        {
            _tableStorageService = tableStorageService;
            _ridesTableStorage = ridesTableStorage;
            _driversVerificationTableStorage = driversVerificationTableStorage;
            _updateUserQueueService = updateUserQueueService;
            _updateUserResponseQueueService = updateUserResponseQueue;
            _driverApplicationQueueService = driverApplicationQueueService;
            _tokenGenerateService = tokenGenerateService;
            _rideQueueService = rideQueueService;
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
            try
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
            catch (Exception e)
            {
                Console.WriteLine("Error Approve driver : " + e.Message);
                throw;
            }
        }

        [HttpPost("reject-drivers-request")]
        public async Task<IActionResult> RejectRequest([FromBody] UserIdDTO idDTO)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine("Error reject request : " + e.Message);
                throw;
            }
        }
        [HttpGet("get-all-rides")]
        public async Task<IActionResult> GetAllRides()
        {
            try
            {
                await LoadData("r");
                return Ok(new { allRides });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
        }
        [HttpPost("order-a-ride")]
        public async Task<IActionResult> HandleOrderRide([FromBody] RideDTO rideDto)
        {
            try
            {
                await _rideQueueService.QueueRideAsync(rideDto);
                string respone = await _rideQueueService.QueueCreateRideResponseAsync();
                if (respone.Equals("success")) 
                     return Ok(new { success = true });
                else
                    return Ok(new { success = false });
            }
            catch (Exception e) 
            {
                Console.WriteLine("Error reject request : " + e.Message);
                throw;
            }
        }
        [HttpPost("accept-ride")]
        public async Task<IActionResult> AcceptRide([FromBody] AcceptRideDataDTO data)
        {
            try
            {
                await _rideQueueService.QueueAcceptRideAsync(data);
                string respone = await _rideQueueService.QueueAcceptRideResponseAsync();
                if (respone.Equals("success"))
                    return Ok(new { success = true });
                else
                    return Ok(new { success = false });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reject request : " + e.Message);
                throw;
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
                    case "r": allRides = await _ridesTableStorage.RetrieveAllRidesAsync();
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
