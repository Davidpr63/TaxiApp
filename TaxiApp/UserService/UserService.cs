using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using UserService.CloudStorageService;
using UserService.DTOModels;
using UserService.IUserService;
using UserService.Models;
using UserService.QueueUserServiceCommunication;
using static System.Fabric.FabricClient;

namespace UserService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public class UserService : StatefulService, IUserOperationService
    {
        private readonly QueueClient _updateUserQueue;
        private readonly QueueClient _driversVerificationQueue;
        private readonly QueueClient _approveDriverQueue;
        private readonly QueueClient _rejectRequestQueue;
        private readonly UpdateUserResponseQueueService _updateUserResponseQueueService;
        private readonly ApproveDriverResponseQueueService _approveDriverResponseQueueService;
        private readonly RejectRequestResponseQueueService _rejectRequestResponseQueueService;
        private BlobStorageService _blobStorageService;
        private TableStorageService _tableStorageService;
        private DriversVerificationTableStorage _driversVerificationTableStorage;
        private DriversVerificationResponseQueueService _driversVerificationResponseQueueService;
        private List<DriverVerification> allVerifications = new List<DriverVerification>();
        private List<User> allUsers = new List<User>();
        public UserService(StatefulServiceContext context)
            : base(context) 
        {
            var settings = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            #region QueueStorage
                string connectionString = settings.Settings.Sections["AzureStorageQueue"].Parameters["StorageConnectionString"].Value;
                string UpdateUserQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["UpdateUserQueueName"].Value;
                string DriverVerificationQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["DriversVerificationQueueName"].Value;
                string UpdateUserResponseQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["UpdateUserResponseQueueName"].Value;
                string DriverVerificationResponseQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["DriversVerificationResponseQueueName"].Value;
                string approveDriverQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["ApproveDriverQueueName"].Value;
                string approveDriverResponseQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["ApproveDriverResponseQueueName"].Value;
                string rejectRequestQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["RejectRequestQueueName"].Value;
                string rejectRequestResponseQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["RejectRequestResponseQueueName"].Value;

            #endregion

            #region table&blob storage
            string blobContainerName = settings.Settings.Sections["AzureStorage"].Parameters["BlobContainer"].Value;
            string tableName = settings.Settings.Sections["AzureStorage"].Parameters["UsersTable"].Value;
            string driversVerificationtableName = settings.Settings.Sections["AzureStorage"].Parameters["DriversVerificationTable"].Value;
            _blobStorageService = new BlobStorageService(connectionString, blobContainerName);
            _tableStorageService = new TableStorageService(connectionString, tableName);
            _driversVerificationTableStorage = new DriversVerificationTableStorage(connectionString, driversVerificationtableName);
            #endregion

            _updateUserQueue = new QueueClient(connectionString, UpdateUserQueueName);
            _updateUserQueue.CreateIfNotExists();
            _driversVerificationQueue = new QueueClient(connectionString, DriverVerificationQueueName);
            _driversVerificationQueue.CreateIfNotExists();
            _approveDriverQueue = new QueueClient(connectionString, approveDriverQueueName);
            _approveDriverQueue.CreateIfNotExists();
            _rejectRequestQueue = new QueueClient(connectionString, rejectRequestQueueName);
            _rejectRequestQueue.CreateIfNotExists();

            _updateUserResponseQueueService = new UpdateUserResponseQueueService(connectionString, UpdateUserResponseQueueName);
            _driversVerificationResponseQueueService = new DriversVerificationResponseQueueService(connectionString, DriverVerificationResponseQueueName);
            _approveDriverResponseQueueService = new ApproveDriverResponseQueueService(connectionString, approveDriverResponseQueueName);
            _rejectRequestResponseQueueService = new RejectRequestResponseQueueService(connectionString, rejectRequestResponseQueueName);
            

            Task.Run(() => ProcessQueueMessagesAsync());
        }
        private async Task ProcessQueueMessagesAsync()
        {
            while (true)
            {
                try
                {
                    QueueMessage[] UpdateUserQueueMessages = await _updateUserQueue.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));
                    QueueMessage[] DVQueueMessages = await _driversVerificationQueue.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));
                    QueueMessage[] ADQueueMessages = await _approveDriverQueue.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));
                    QueueMessage[] RRQueueMessages = await _rejectRequestQueue.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));


                    if (UpdateUserQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in UpdateUserQueueMessages)
                        {

                            var loginUser = JsonConvert.DeserializeObject<UserDTO>(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)));
                            await UpdateUserAsync(loginUser);
                            await _updateUserQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                    }
                    if (DVQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in DVQueueMessages)
                        {

                            var userId = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)));
                            await HandleDriversVerification(userId);
                            await _driversVerificationQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                    }
                    if (ADQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in ADQueueMessages)
                        {

                            var userId = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)));
                            await ApproveDriver(userId);
                            await _approveDriverQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                    }
                    if (RRQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in RRQueueMessages)
                        {

                            var userId = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)));
                            await RejectDriversRequest(userId);
                            await _rejectRequestQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                    }

                }
                catch (Exception ex)
                {

                    ServiceEventSource.Current.ServiceMessage(Context, $"Exception in queue processing: {ex.Message}");
                }

                // Wait before polling for new messages.
                await Task.Delay(1000);
            }
        }
        

        public async Task UpdateUserAsync(UserDTO updateUserData)
        {
            try
            {
                await LoadData();
                string imageUrl = "";
                User loggedInUser = allUsers.FirstOrDefault(x => x.RowKey == updateUserData.UserId);
                if (!string.IsNullOrEmpty(updateUserData.Password) || !string.IsNullOrEmpty(updateUserData.ConfirmPassword) && updateUserData.Password != updateUserData.ConfirmPassword)
                {
                    //return Ok(new { success = false, error = "Passwords dont match!" });
                    await _updateUserResponseQueueService.QueueUpdateUserResponseAsync("Passwords dont match!");
                }


                if (!string.IsNullOrEmpty(updateUserData.Email) && updateUserData.Email != loggedInUser.Email)
                {
                    if (!ExistUser(updateUserData.Email))
                    {
                        loggedInUser.Email = updateUserData.Email;
                    }
                    else
                    {
                        // return Ok(new { success = false, error = "That email already exists, please use another one." });
                        await _updateUserResponseQueueService.QueueUpdateUserResponseAsync("That email already exists, please use another one!");
                    }
                }
                if (!string.IsNullOrEmpty(updateUserData.Firstname))
                {
                    loggedInUser.Firstname = updateUserData.Firstname;
                }
                if (!string.IsNullOrEmpty(updateUserData.Lastname))
                {
                    loggedInUser.Lastname = updateUserData.Lastname;
                }
                if (!string.IsNullOrEmpty(updateUserData.Username))
                {
                    loggedInUser.Username = updateUserData.Username;
                }


                //      AuthController.UserTemp.Email = updateUserData.Email;
                if (!string.IsNullOrEmpty(updateUserData.Password))
                {
                    loggedInUser.Password = HashPassword(updateUserData.Password);
                }
                if (!string.IsNullOrEmpty(updateUserData.DateOfBirth))
                {
                    loggedInUser.DateOfBirth = updateUserData.DateOfBirth;
                }
                if (!string.IsNullOrEmpty(updateUserData.Address))
                {
                    loggedInUser.Address = updateUserData.Address;
                }

                if (updateUserData.Image != null)
                {
                    imageUrl = await _blobStorageService.UploadImageAsync(updateUserData.Image);
                    loggedInUser.ImageUrl = imageUrl;
                }

                await _tableStorageService.UpdateUserAsync(loggedInUser);
                await _updateUserResponseQueueService.QueueUpdateUserResponseAsync("success");

            }
            catch (Exception e)
            {
                    Console.WriteLine(e.Message);
                    return;
            }
        }
        public async Task HandleDriversVerification(string id)
        {
            try
            {
                await LoadData();
                User user = allUsers.FirstOrDefault(x => x.RowKey == id);
                int rowKey = 0;
                if (user != null)
                {
                    if (allVerifications.Count() == 0)
                    {
                        rowKey = 1;
                    }
                    else
                        rowKey = allVerifications.Count() + 1;

                    DriverVerification driverVerification = new DriverVerification()
                    {
                        RowKey = rowKey.ToString(),
                        DriversEmail = user.Email,
                        DriversName = user.Firstname,
                        DriversLastname = user.Lastname,
                        UserId = user.RowKey,
                        VerificationStatus = "In process"
                        
                        
                    };
                    user.VerifcationStatus = "In process";
                    await _tableStorageService.UpdateUserAsync(user);
                    await _driversVerificationTableStorage.CreateVerificationAsync(driverVerification);

                    await _driversVerificationResponseQueueService.DriversVerificationResponseAsync("success");
                }

            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return;
            }
        }
        public async Task ApproveDriver(string id)
        {
            try
            {
                await LoadData();
                User user = allUsers.FirstOrDefault(x => x.RowKey == id);
                DriverVerification verification = allVerifications.FirstOrDefault(x => x.UserId == id);
                if (verification != null && user != null)
                {
                    user.VerifcationStatus = "Approved";
                    user.TypeOfUser = TypeOfUser.Driver;
                    verification.VerificationStatus = "Approved";
                    await _tableStorageService.UpdateUserAsync(user);
                    await _driversVerificationTableStorage.UpdateVerificationAsync(verification);
                    await SendEmail(user.Email, "Your request has been accepted, you have become a driver");
                    await _approveDriverResponseQueueService.ApproveDriverResponseAsync("success");
                }
                else
                    await _approveDriverResponseQueueService.ApproveDriverResponseAsync("something wrong");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
        public async Task RejectDriversRequest(string id)
        {
            try
            {
                await LoadData();
                User user = allUsers.FirstOrDefault(x => x.RowKey == id);
                DriverVerification verification = allVerifications.FirstOrDefault(x => x.UserId == id);
                if (verification != null && user != null)
                {
                    user.VerifcationStatus = "Rejected";
                    verification.VerificationStatus = "Rejected";
                    await _tableStorageService.UpdateUserAsync(user);
                    await _driversVerificationTableStorage.UpdateVerificationAsync(verification);
                    await SendEmail(user.Email, "Your request has been rejected");
                    await _rejectRequestResponseQueueService.RejectRequestResponseAsync("success");
                }
                else
                    await _rejectRequestResponseQueueService.RejectRequestResponseAsync("success");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        public async Task SendEmail(string toEmail, string text)
        {
            string smtpServer = "smtp.gmail.com";
            int port = 587;

            
            string username = "david.02.petrovic@gmail.com";
            string password = "tqsd xoav dsyz jlgi";
            string fromEmail = "david.02.petrovic@gmail.com";
            SmtpClient client = new SmtpClient(smtpServer, port);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(username, password);


            MailMessage message = new MailMessage(fromEmail, toEmail);
            message.Subject = $"Drivers verification";
            message.Body = text;

            try
            {
                // Slanje email poruke
                client.Send(message);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public string HashPassword(string password)
        {
            try
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {

                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));


                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        
        public bool ExistUser(string email)
        {
            bool result = false;

            foreach (var item in allUsers)
            {
                if (item.Email == email)
                {
                    result = true;
                    return result;
                }
            }
            return result;
        }
        public async Task LoadData()
        {
            try
            {
                allUsers = await _tableStorageService.RetrieveAllUsersAsync();
                allVerifications = await _driversVerificationTableStorage.RetrieveAllVerificationsAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("LodaData error");

            }
        }

      
    }
}
