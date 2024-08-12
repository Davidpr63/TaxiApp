using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using System.Text;
using System.Fabric.Description;

namespace ApiGatewayService.QueueApiServiceCommunication
{
    public class LoginResponseQueue
    {
        private readonly QueueClient _queueClient;
        public LoginResponseQueue(string connectionString, string queueName)
        {

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }
        public async Task<bool> QueueLoginResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] LoginResponseQueueMessages = await _queueClient.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (LoginResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in LoginResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            if (response.Equals("success"))
                            {
                                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                                return true;
                            }
                            else
                            {
                                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                                return false;
                            }


                        }
                    }

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                    return false;
                }


            }
        }
    }
}
