using ApiGatewayService.DTOmodels;
using ApiGatewayService.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueApiServiceCommunication
{
    public class RegistrationQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly QueueClient _queueClientResponse;

        public RegistrationQueueService(IConfiguration configuration)
        {
           
            _queueClient = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:RegistrationQueueName"]);
            _queueClient.CreateIfNotExists();

            _queueClientResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:RegistrationResponseQueueName"]);
            _queueClientResponse.CreateIfNotExists();
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
        public async Task<string> QueueRegistrationUserResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] RUResponseQueueMessages = await _queueClientResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (RUResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in RUResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueClientResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                            return response;


                        }
                    }

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                    return "false";
                }


            }
        }
    }
}
