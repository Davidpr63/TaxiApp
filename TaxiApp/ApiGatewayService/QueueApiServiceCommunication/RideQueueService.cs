using ApiGatewayService.DTOmodels;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System.Text;

namespace ApiGatewayService.QueueApiServiceCommunication
{
    public class RideQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly QueueClient _queueResponse;
        private readonly QueueClient _queueAcceptRide;
        private readonly QueueClient _queueAcceptRideResponse;

        public RideQueueService(IConfiguration configuration)
        {

            _queueClient = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:RideQueueName"]);
            _queueClient.CreateIfNotExists();
            _queueResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:CreateRideResponseQueueName"]);
            _queueResponse.CreateIfNotExists();

            _queueAcceptRide = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:AcceptRideQueueName"]);
            _queueAcceptRide.CreateIfNotExists();
            _queueAcceptRideResponse = new QueueClient(configuration["AzureStorage:ConnectionString"], configuration["AzureStorage:AcceptideResponseQueueName"]);
            _queueAcceptRideResponse.CreateIfNotExists();
        }

        public async Task QueueRideAsync(RideDTO rideDto)
        {
            try
            {
                var message = JsonConvert.SerializeObject(rideDto);
                await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

        }
        public async Task<string> QueueCreateRideResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] CRResponseQueueMessages = await _queueResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (CRResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in CRResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
        public async Task QueueAcceptRideAsync(AcceptRideDataDTO data)
        {
            try
            {
                var message = JsonConvert.SerializeObject(data);
                await _queueAcceptRide.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

        }
        public async Task<string> QueueAcceptRideResponseAsync()
        {
            while (true)
            {
                try
                {

                    QueueMessage[] ARResponseQueueMessages = await _queueAcceptRideResponse.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));

                    if (ARResponseQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in ARResponseQueueMessages)
                        {

                            var response = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                            await _queueAcceptRideResponse.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
