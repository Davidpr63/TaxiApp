using ApiGatewayService.DTOmodels;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueApiServiceCommunication
{
    public class LoginQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly QueueClient _queueClientResponse;

        public LoginQueueService(IConfiguration configuration)
        {

            _queueClient = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:LoginQueueName"]);
            _queueClient.CreateIfNotExists();
            _queueClientResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:LoginResponseQueueName"]);
            _queueClientResponse.CreateIfNotExists();
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
        public async Task<string> QueueLoginResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] LoginResponseQueueMessages = await _queueClientResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (LoginResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in LoginResponseQueueMessages)
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
