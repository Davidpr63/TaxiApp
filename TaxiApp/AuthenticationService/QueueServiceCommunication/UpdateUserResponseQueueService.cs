using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationService.QueueServiceCommunication
{
    public class UpdateUserResponseQueueService
    {
        private readonly QueueClient _queueClient;

        public UpdateUserResponseQueueService(string connectionString, string queueName)
        {

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task QueueUpdateUserResponseAsync(string response)
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
