using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService.wcf
{
    public static class WcfUtility
    {
        public static NetTcpBinding CreateTcpListenerBinding()
        {
            return new NetTcpBinding(SecurityMode.None)
            {
                //MaxConnections = 10,
                MaxBufferPoolSize = 524288,
                MaxBufferSize = 65536,
                MaxReceivedMessageSize = 65536
            };
        }
    }
}
