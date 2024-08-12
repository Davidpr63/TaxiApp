using ApiGatewayService.DTOmodels;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueServiceCommunication
{
    public class DriverApplicationQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly QueueClient _queueClientResponse;
        private readonly QueueClient _queueApproveDriver;
        private readonly QueueClient _queueApproveDriverResponse;
        private readonly QueueClient _queueRejectRequest;
        private readonly QueueClient _queueRejectRequestResponse;

        public DriverApplicationQueueService(IConfiguration configuration)
        {
            
            _queueClient = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:DriversVerificationQueueName"]);
            _queueClient.CreateIfNotExists();

            _queueClientResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:DriversVerificationResponseQueueName"]);
            _queueClientResponse.CreateIfNotExists();

            _queueApproveDriver = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:ApproveDriverQueueName"]);
            _queueApproveDriver.CreateIfNotExists();

            _queueApproveDriverResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:ApproveDriverResponseQueueName"]);
            _queueApproveDriverResponse.CreateIfNotExists();

            _queueRejectRequest = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:RejectRequestQueueName"]);
            _queueRejectRequest.CreateIfNotExists();

            _queueRejectRequestResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:RejectRequestResponseQueueName"]);
            _queueRejectRequestResponse.CreateIfNotExists();
        }

        public async Task QueueDriverApplicationAsync(string id)
        {
            try
            {
                var message = JsonConvert.SerializeObject(id);
                await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }

        }
        public async Task<string> QueueDriversVerificationResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] DVResponseQueueMessages = await _queueClientResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (DVResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in DVResponseQueueMessages)
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
        public async Task QueueApproveDriverAsync(string id)
        {
            try
            {
                var message = JsonConvert.SerializeObject(id);
                await _queueApproveDriver.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

        }
        public async Task<string> QueueApproveDriverResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] ADResponseQueueMessages = await _queueApproveDriverResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (ADResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in ADResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueApproveDriverResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
        public async Task QueueRejectRequestAsync(string id)
        {
            try
            {
                var message = JsonConvert.SerializeObject(id);
                await _queueRejectRequest.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

        }
        public async Task<string> QueueRejectRequestResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] RRResponseQueueMessages = await _queueRejectRequestResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (RRResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in RRResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueRejectRequestResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
