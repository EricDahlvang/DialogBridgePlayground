using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using V3Migration;

namespace V4NetCoreBot
{
    public class Startup
    {
        private bool _isProduction = false;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            BotConfiguration botConfig = null;
            try
            {
                botConfig = BotConfiguration.Load(botFilePath ?? @".\V4NetCoreBot.bot", secretKey);
            }
            catch
            {
                var msg = @"Error reading bot file. Please ensure you have valid botFilePath and botFileSecret set for your environment.
                        - You can find the botFilePath and botFileSecret in the Azure App Service application settings.
                        - If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
                        - See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
                        ";
                throw new InvalidOperationException(msg);
            }
            
            services.AddSingleton(sp => GetCredentialProvider(botConfig));
            services.AddSingleton(sp => botConfig);
            services.AddSingleton(sp => GetAccessors());
        }

        private ICredentialProvider GetCredentialProvider(BotConfiguration botConfig)
        {
            var environment = _isProduction ? "production" : "development";
            var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
            }

            return new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
        }

        private BotAccessors GetAccessors()
        {
            IStorage dataStore = new MemoryStorage();

            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);
            var privateConversationState = new PrivateConversationState(dataStore);

            // Create the custom state accessor.
            // State accessors enable other components to read and write individual properties of state.
            return new BotAccessors(conversationState, privateConversationState, userState)
            {
                UserData = userState.CreateProperty<BotDataBag>(BotAccessors.UserDataPropertyName),
                ConversationData = conversationState.CreateProperty<BotDataBag>(BotAccessors.ConversationDataPropertyName),
                PrivateConversationData = privateConversationState.CreateProperty<BotDataBag>(BotAccessors.PrivateConversationDataPropertyName),
                DialogData = conversationState.CreateProperty<DialogState>(nameof(DialogState))
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            _isProduction = env.IsProduction();

            app.UseMvc();
        }
    }
}
