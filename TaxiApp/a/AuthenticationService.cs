using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationService.IAuthentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.ServiceModel;

namespace AuthenticationService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public class AuthenticationService : StatelessService, IAuthenticationService
    {
        public AuthenticationService(StatelessServiceContext context)
             : base(context)
        { }

        public async Task RegisterUserAsync(string email, string password)
        {
            throw new NotImplementedException();
        }
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context =>
                    new WcfCommunicationListener<IAuthenticationService>(
                        serviceContext: context,
                        wcfServiceObject: this,
                        listenerBinding: WcfUtility.CreateTcpListenerBinding(),
                        endpointResourceName: "WcfServiceEndpoint"
                    ))
            };
        }
        public class Startup { }
    }
}
