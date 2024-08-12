using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using System.Text;
using System.Fabric.Description;

namespace ApiGatewayService.QueueApiServiceCommunication
{
    public class UpdateUserResponseQueue
    {
        private readonly QueueClient _queueClient;
        public UpdateUserResponseQueue(string connectionString, string queueName)
        {

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }
        public async Task<string> QueueUpdateUserResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] UpdateUserResponseQueueMessages = await _queueClient.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (UpdateUserResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in UpdateUserResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)); 
                            await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
