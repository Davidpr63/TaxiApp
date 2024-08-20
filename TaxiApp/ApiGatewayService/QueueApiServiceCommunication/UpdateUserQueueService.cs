using ApiGatewayService.DTOmodels;
using ApiGatewayService.Models;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueApiServiceCommunication
{
    public class UpdateUserQueueService
    {
        private readonly QueueClient _queueClient;

        public UpdateUserQueueService(string connectionString, string queueName)
        {

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task QueueUpdateUserAsync(User user)
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
