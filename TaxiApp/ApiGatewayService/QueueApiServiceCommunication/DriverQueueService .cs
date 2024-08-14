using ApiGatewayService.DTOmodels;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueServiceCommunication
{
    public class DriverQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly QueueClient _queueClientResponse;
        private readonly QueueClient _queueApproveDriver;
        private readonly QueueClient _queueApproveDriverResponse;
        private readonly QueueClient _queueRejectRequest;
        private readonly QueueClient _queueRejectRequestResponse;
        private readonly QueueClient _queueDriverRating;
        private readonly QueueClient _queueDriverRatingResponse;
        private readonly QueueClient _queueBlockDriver;
        private readonly QueueClient _queueBlockDriverResponse;
        private readonly QueueClient _queueUnBlockDriver;
        private readonly QueueClient _queueUnBlockDriverResponse;

        public DriverQueueService(IConfiguration configuration)
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

            _queueDriverRating = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:DriverRatingQueueName"]);
            _queueDriverRating.CreateIfNotExists();

            _queueDriverRatingResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:DriverRatingResponseQueueName"]);
            _queueDriverRatingResponse.CreateIfNotExists();

            _queueBlockDriver = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:BlockDriverQueueName"]);
            _queueBlockDriver.CreateIfNotExists();

            _queueBlockDriverResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:BlockDriverResponseQueueName"]);
            _queueBlockDriverResponse.CreateIfNotExists();

            _queueUnBlockDriver = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:UnBlockDriverQueueName"]);
            _queueUnBlockDriver.CreateIfNotExists();

            _queueUnBlockDriverResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:UnBlockDriverResponseQueueName"]);
            _queueUnBlockDriverResponse.CreateIfNotExists();
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
        public async Task QueueDriverRatingAsync(DriverRatingDTO dto)
        {
            try
            {
                var message = JsonConvert.SerializeObject(dto);
                await _queueDriverRating.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

        }
        public async Task<string> QueueDriverRatingResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] DRResponseQueueMessages = await _queueDriverRatingResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (DRResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in DRResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueDriverRatingResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
        public async Task QueueBlockDriverAsync(string id)
        {
            try
            {
                var message = JsonConvert.SerializeObject(id);
                await _queueBlockDriver.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

        }
        public async Task<string> QueueBlockDriverResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] BDResponseQueueMessages = await _queueBlockDriverResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (BDResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in BDResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueBlockDriverResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
        public async Task QueueUnBlockDriverAsync(string id)
        {
            try
            {
                var message = JsonConvert.SerializeObject(id);
                await _queueUnBlockDriver.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

        }
        public async Task<string> QueueUnBlockDriverResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] UBDesponseQueueMessages = await _queueUnBlockDriverResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (UBDesponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in UBDesponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueUnBlockDriverResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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