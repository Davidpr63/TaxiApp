using ApiGatewayService.CloudStorageService;
using ApiGatewayService.Common;
using ApiGatewayService.DTOmodels;
using ApiGatewayService.Models;
using ApiGatewayService.QueueApiServiceCommunication;
using ApiGatewayService.QueueServiceCommunication;
using ApiGatewayService.TokenService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric.Management.ServiceModel;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiGatewayService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UpdateUserQueueService _updateUserQueueService;
        private readonly UpdateUserResponseQueue _updateUserResponseQueueService;
        private readonly DriversVerificationTableStorage _driversVerificationTableStorage;
        private readonly DriverQueueService _driverQueueService;
        private readonly RideQueueService _rideQueueService;
        private TokenGenerateService _tokenGenerateService;
        private TableStorageService _tableStorageService;
        private RidesTableStorage _ridesTableStorage;
        private List<User> users = new List<User>();
        private List<DriverVerification> driverVerifications = new List<DriverVerification>();
        private List<Ride> allRides = new List<Ride>();
        public UserController(RidesTableStorage ridesTableStorage, RideQueueService rideQueueService, DriverQueueService driverApplicationQueueService, DriversVerificationTableStorage driversVerificationTableStorage, UpdateUserQueueService updateUserQueueService, UpdateUserResponseQueue updateUserResponseQueue,TableStorageService tableStorageService, TokenGenerateService tokenGenerateService)
        {
            _tableStorageService = tableStorageService;
            _ridesTableStorage = ridesTableStorage;
            _driversVerificationTableStorage = driversVerificationTableStorage;
            _updateUserQueueService = updateUserQueueService;
            _updateUserResponseQueueService = updateUserResponseQueue;
            _driverQueueService = driverApplicationQueueService;
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
                    await _driverQueueService.QueueDriverApplicationAsync(idDTO.UserId); 
                }
                else
                    return Ok(new { success = false, error = "id is null" });
                string respone = await _driverQueueService.QueueDriversVerificationResponseAsync();
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
                    await _driverQueueService.QueueApproveDriverAsync(idDTO.UserId);
                }
                else
                    return Ok(new { success = false, error = "id is null" });
                string respone = await _driverQueueService.QueueApproveDriverResponseAsync();
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
                    await _driverQueueService.QueueRejectRequestAsync(idDTO.UserId);
                }
                else
                    return Ok(new { success = false, error = "id is null" });
                string respone = await _driverQueueService.QueueRejectRequestResponseAsync();
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
                    return Ok(new { success = false, respone });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reject request : " + e.Message);
                throw;
            }
        }
        [HttpPost("check-ride-status")]
        public async Task<IActionResult> CheckRideStatus([FromBody] UserIdDTO Dto)
        {
            try
            {
                await LoadData("r");
                Ride ride = allRides.FirstOrDefault(x => x.IsAccepted == true && x.IsActive == true && Dto.UserId == x.UserId);
                if (ride != null)
                {
                    ride.IsActive = false;
                    await _ridesTableStorage.UpdateRideAsync(ride);
                    return Ok(new { status = true });
                }
                else
                    return Ok(new { status = false });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reject request : " + e.Message);
                throw;
            }
        }
        
        [HttpPost("driver-rating")]
        public async Task<IActionResult> DriverRate([FromBody] DriverRatingDTO Dto)
        {
            try
            {
                //ServicePartitionKey partition = new ServicePartitionKey(long.Parse("1"));
                //var statefulProxy = ServiceProxy.Create<IUserOperation>(
                //    new Uri("fabric:/TaxiApp/UserService"),
                //    partition
                //);
                //Console.WriteLine($"Proces pokrenut: {statefulProxy}");
                //var result = statefulProxy.DriverRating(Dto);
              
                await _driverQueueService.QueueDriverRatingAsync(Dto);
                string respone = await _driverQueueService.QueueDriverRatingResponseAsync();
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
        [HttpPost("block-driver")]
        public async Task<IActionResult> BlockDriver([FromBody] UserIdDTO Dto)
        {
            try
            {
                

                await _driverQueueService.QueueBlockDriverAsync(Dto.UserId);
                string respone = await _driverQueueService.QueueBlockDriverResponseAsync();
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
        [HttpPost("unblock-driver")]
        public async Task<IActionResult> UnBlockDriver([FromBody] UserIdDTO Dto)
        {
            try
            {


                await _driverQueueService.QueueUnBlockDriverAsync(Dto.UserId);
                string respone = await _driverQueueService.QueueUnBlockDriverResponseAsync();
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
        [HttpPost("get-previous-rides")]
        public async Task<IActionResult> GetPreviousRides([FromBody]UserIdDTO dto)
        {
            try
            {
                await LoadData("rr");
                List<Ride> PreviousRides = allRides.Where(x => x.UserId == dto.UserId && x.IsActive == false).ToList();
                if (PreviousRides != null)
                {
                    return Ok(new { PreviousRides });
                }
                else
                    return Ok();
               

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
        }
        [HttpPost("drivers-rides")]
        public async Task<IActionResult> GetDriversRides([FromBody] UserIdDTO dto)
        {
            try
            {
                await LoadData("rr");
                List<Ride> DriversRides = allRides.Where(x => x.DriverId.ToString() == dto.UserId && x.IsActive == false).ToList();
                if (DriversRides != null)
                {
                    return Ok(new { DriversRides });
                }
                else
                    return Ok();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
            }
        }
        [HttpGet("all-rides")]
        public async Task<IActionResult> GetRidesForAdmin()
        {
            try
            {
                await LoadData("rr");

                if (allRides != null)
                {
                    return Ok(new { allRides });
                }
                else
                    return Ok();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Register exception :" + e.Message);
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
                    case "r": allRides = await _ridesTableStorage.RetrieveAllRidesCustomAsync();
                        break;
                    case "rr":
                        allRides = await _ridesTableStorage.RetrieveAllRidesAsync();
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
