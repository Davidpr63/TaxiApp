using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Fabric;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationService.IAuth;
using AuthenticationService.DTOmodels;
using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;

using Microsoft.ServiceFabric.Services.Runtime;

using Newtonsoft.Json;
using System.Xml.Linq;
using Azure.Data.Tables;
using AuthenticationService.CloudStorageService;
using AuthenticationService.Models;
using System.Security.Cryptography;
using ApiGatewayService.QueueServiceCommunication;
using AuthenticationService.QueueServiceCommunication;
namespace AuthenticationService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public class AuthenticationService: StatelessService, IAuthentication
    {
        private readonly QueueClient _registrationQueue;
        private readonly QueueClient _loginQueue;
        private readonly QueueClient _updateUserQueue;
        private readonly LoginResponseQueueService _loginResponseQueue;
        private readonly UpdateUserResponseQueueService _updateUserResponseQueueService;
        private TableStorageService _tableStorageService;
        private BlobStorageService _blobStorageService;
        private List<User> allUsers = new List<User>();
        public AuthenticationService(StatelessServiceContext context)
        : base(context)
        {
       
            var settings = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            string connectionString = settings.Settings.Sections["AzureStorageQueue"].Parameters["StorageConnectionString"].Value;
            string queueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["RegistrationQueueName"].Value;

            string loginQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["LoginQueueName"].Value;
            string UpdateUserQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["UpdateUserQueueName"].Value;
       
            string tableName = settings.Settings.Sections["AzureStorage"].Parameters["UsersTable"].Value;
            string blobContainerName = settings.Settings.Sections["AzureStorage"].Parameters["BlobContainer"].Value;

            string loginResponseQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["LoginResponseQueueName"].Value; 

            string UpdateUserResponseQueueName = settings.Settings.Sections["AzureStorageQueue"].Parameters["UpdateUserResponseQueueName"].Value; 

            _registrationQueue = new QueueClient(connectionString, queueName);
            _registrationQueue.CreateIfNotExists();

            _loginQueue = new QueueClient(connectionString, loginQueueName);
            _loginQueue.CreateIfNotExists();

            _updateUserQueue = new QueueClient(connectionString, UpdateUserQueueName);
            _updateUserQueue.CreateIfNotExists();


            _loginResponseQueue = new LoginResponseQueueService(connectionString, loginResponseQueueName);
            _updateUserResponseQueueService = new UpdateUserResponseQueueService(connectionString, UpdateUserResponseQueueName);
            

            _tableStorageService = new TableStorageService(connectionString, tableName);
            _blobStorageService = new BlobStorageService(connectionString, blobContainerName);
            Task.Run(() => ProcessQueueMessagesAsync());
        }
        private async Task ProcessQueueMessagesAsync()
        {
            while (true)
            {
                try
                {
                 
                    QueueMessage[] RegQueueMessages = await _registrationQueue.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));
                    QueueMessage[] LoginQueueMessages = await _loginQueue.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));
                    QueueMessage[] UpdateUserQueueMessages = await _updateUserQueue.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30));
            
                    if (RegQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in RegQueueMessages)
                        {

                            var newUser = JsonConvert.DeserializeObject<User>(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)));
                            await RegisterUserAsync(newUser);
                            await _registrationQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                    }
                    if (LoginQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in LoginQueueMessages)
                        {

                            var loginUser = JsonConvert.DeserializeObject<LoginDTOUser>(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)));
                            await LoginUserAsync(loginUser);
                            await _loginQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                    }
                    if (UpdateUserQueueMessages.Length > 0)
                    {
                        foreach (QueueMessage message in UpdateUserQueueMessages)
                        {

                            var loginUser = JsonConvert.DeserializeObject<UserDTO>(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText)));
                            await UpdateUserAsync(loginUser);
                            await _loginQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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
        public async Task RegisterUserAsync(User newUser)
        {
            try
            {
                _tableStorageService.CreateUserAsync(newUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Registration erro :" + ex.Message);
                return;
            }
            
        }
        private User loggedInUser = new User();
        public async Task LoginUserAsync(LoginDTOUser loginDTOUser)
        {
            try
            {
                await LoadData();

                loggedInUser = allUsers.FirstOrDefault(x => x.Email == loginDTOUser.Email && x.Password == HashPassword(loginDTOUser.Password));

                if (loggedInUser != null)
                {
                    //   HttpContext.Session.SetString("TypeOfUser", "Admin");
                    //  HttpContext.Session.SetObjectAsJson("LoggedInUser", loggedInUser);

                    await _loginResponseQueue.QueueLoginResponseAsync("success");
                }
                else
                {
                    await _loginResponseQueue.QueueLoginResponseAsync("failed");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        public async Task UpdateUserAsync(UserDTO updateUserData)
        {
            try
            {
                await LoadData();
                string imageUrl = "";






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
            }
            catch (Exception)
            {
                Console.WriteLine("LodaData error");

            }
        }
    }
}
