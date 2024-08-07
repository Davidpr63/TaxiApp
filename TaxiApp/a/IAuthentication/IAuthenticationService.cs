using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService.IAuthentication
{
    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        Task RegisterUserAsync(string email, string password);
    }
}
