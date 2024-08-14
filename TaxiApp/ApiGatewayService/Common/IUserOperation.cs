using ApiGatewayService.DTOmodels;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ApiGatewayService.Common
{
    public interface IUserOperation : IService
    {
        Task DriverRating(DriverRatingDTO rate);
    }
}
