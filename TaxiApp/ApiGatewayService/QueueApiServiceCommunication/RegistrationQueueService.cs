using ApiGatewayService.DTOmodels;
using ApiGatewayService.Models;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueApiServiceCommunication
{
    public class RegistrationQueueService
    {
        private readonly QueueClient _queueClient;

        public RegistrationQueueService(string connectionString, string queueName)
        {
           
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task QueueUserRegistrationAsync(User user)
        {
            try
            {
                var message = JsonConvert.SerializeObject(user);
                await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            
        }
    }
}
