using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ApiGatewayService.TokenService;
using System.Fabric;
using ApiGatewayService.QueueApiServiceCommunication;
using ApiGatewayService.CloudStorageService;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ApiGatewayService.QueueServiceCommunication;

namespace ApiGatewayService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ApiGatewayService : StatelessService
    {
        public ApiGatewayService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.Services.AddSingleton<TableStorageService>();
                        builder.Services.AddSingleton<BlobStorageService>();
                        
                        builder.Services.AddSingleton<DriversVerificationTableStorage>();
                        builder.Services.AddSingleton<RidesTableStorage>();
                        //builder.Services.AddSingleton<IAuthentication>(provider =>
                        //{
                        //    var serviceUri = new Uri("fabric:/TaxiApp/AuthenticationService");
                        //    return ServiceProxy.Create<IAuthentication>(serviceUri);
                        //});
                        builder.Services.AddSingleton<RegistrationQueueService>(provider =>
                        {
                            var configuration = provider.GetRequiredService<IConfiguration>();

                        
                            var connectionString = configuration["AzureStorage:ConnectionString"];
                            var queueName = configuration["AzureStorage:RegistrationQueueName"];

                            return new RegistrationQueueService(connectionString, queueName);
                        });
                        builder.Services.AddSingleton<LoginQueueService>(provider =>
                        {
                           var configuration = provider.GetRequiredService<IConfiguration>();


                            var connectionString = configuration["AzureStorage:ConnectionString"];
                            var queueName = configuration["AzureStorage:LoginQueueName"];

                            return new LoginQueueService(connectionString, queueName);
                        });
                        builder.Services.AddSingleton<LoginResponseQueue>(provider =>
                        {
                           var configuration = provider.GetRequiredService<IConfiguration>();


                            var connectionString = configuration["AzureStorage:ConnectionString"];
                            var queueName = configuration["AzureStorage:LoginResponseQueueName"];

                            return new LoginResponseQueue(connectionString, queueName);
                        });
                        builder.Services.AddSingleton<UpdateUserQueueService>(provider =>
                        {
                           var configuration = provider.GetRequiredService<IConfiguration>();


                            var connectionString = configuration["AzureStorage:ConnectionString"];
                            var queueName = configuration["AzureStorage:UpdateUserQueueName"];

                            return new UpdateUserQueueService(connectionString, queueName);
                        });
                        builder.Services.AddSingleton<UpdateUserResponseQueue>(provider =>
                        {
                           var configuration = provider.GetRequiredService<IConfiguration>();


                            var connectionString = configuration["AzureStorage:ConnectionString"];
                            var queueName = configuration["AzureStorage:UpdateUserResponseQueueName"];

                            return new UpdateUserResponseQueue(connectionString, queueName);
                        });
                        builder.Services.AddSingleton<DriverApplicationQueueService>();
                        builder.Services.AddSingleton<RideQueueService>();
                        
                        builder.Services.AddSingleton(new TokenGenerateService(
                                    builder.Configuration["Jwt:Secret"],
                                    builder.Configuration["Jwt:Issuer"],
                                    builder.Configuration["Jwt:Audience"]));
                        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]);
                        builder.Services.AddAuthentication(x =>
                        {
                         
                            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                        .AddJwtBearer(x =>
                        {
                            x.RequireHttpsMetadata = false;
                            x.SaveToken = true;
                            x.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                                ValidAudience = builder.Configuration["Jwt:Audience"],
                                IssuerSigningKey = new SymmetricSecurityKey(key),
                                ClockSkew = TimeSpan.Zero // optional: to avoid delay when token is expired
                            };
                        });

                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);
                        builder.Services.AddControllers();
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();
                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy("AllowSpecificOrigin",
                                builder => builder
                                    .WithOrigins("http://localhost:3000")
                                    .AllowAnyHeader()
                                    .AllowAnyMethod());
                        });
                        var app = builder.Build();
                        if (app.Environment.IsDevelopment())
                        {
                            app.UseSwagger();
                            app.UseSwaggerUI();
                        }

                        app.UseCors("AllowSpecificOrigin");
                        app.UseStaticFiles();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.MapControllers();

                        return app;

                    }))
            };
        }
    }
}
