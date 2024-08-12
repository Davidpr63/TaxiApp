using AuthenticationService.DTOmodels;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using System.Text;

namespace AuthenticationService.QueueAuthServiceCommunication
{
    public class LoginResponseQueueService
    {
        private readonly QueueClient _queueClient;

        public LoginResponseQueueService(string connectionString, string queueName)
        {

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task QueueLoginResponseAsync(string response)
        {
            try
            {
                
                await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(response)));
                 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
              
            }

        }
    }
}
