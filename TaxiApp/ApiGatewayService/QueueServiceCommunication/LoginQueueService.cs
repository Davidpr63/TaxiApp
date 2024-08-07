using ApiGatewayService.DTOmodels;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueServiceCommunication
{
    public class LoginQueueService
    {
        private readonly QueueClient _queueClient;

        public LoginQueueService(string connectionString, string queueName)
        {

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task QueueLoginUserAsync(LoginDTOUser user)
        {
            try
            {
                var message = JsonConvert.SerializeObject(user);
                await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }

        }

       
    }
}
