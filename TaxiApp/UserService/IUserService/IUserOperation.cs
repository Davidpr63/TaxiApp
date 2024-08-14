using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UserService.DTOmodels;

namespace UserService.IUserService
{
    public interface IUserOperation : IService
    {
        Task DriverRating(DriverRatingDTO rate);
    }
}
