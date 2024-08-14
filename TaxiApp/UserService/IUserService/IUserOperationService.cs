using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.DTOmodels;
using UserService.DTOModels;

namespace UserService.IUserService
{
    public interface IUserOperationService
    {
        Task UpdateUserAsync(UserDTO updateUserData);
        Task HandleDriversVerification(string id);
        Task ApproveDriver(string id);
        Task RejectDriversRequest(string id);
        Task SendEmail(string toEmail, string text);
        Task HandleARide(RideDTO rideDto);
        Task AcceptRide(AcceptRideDataDTO Dto);
        Task DriverRating(DriverRatingDTO Dto);
        Task BlockDriver(string id);
        Task UnBlockDriver(string id);
    
    }
}
