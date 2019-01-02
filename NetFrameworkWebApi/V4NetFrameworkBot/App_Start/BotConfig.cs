using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.WebApi;
using Microsoft.Bot.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;

namespace V4NetFrameworkBot
{
    public class BotConfig
    {
        /// <summary>
        /// Register the bot framework with Asp.net.
        /// </summary>
        /// <param name="config">Represents the configuration of the HttpServer.</param>
        public static void Register(HttpConfiguration config)
        {
            var builder = new ContainerBuilder();

            BotAccessors accessors = null;
            config.MapBotFramework(botConfig =>
            {
                // Load Connected Services from .bot file
                var path = HostingEnvironment.MapPath(@"~/V4NetFramework.bot");
                var botConfigurationFile = BotConfiguration.Load(path);
                var endpointService = (EndpointService)botConfigurationFile.Services.First(s => s.Type == "endpoint");
                
                botConfig.UseMicrosoftApplicationIdentity(endpointService?.AppId, endpointService?.AppPassword);

                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, everything stored in memory will be gone.
                IStorage dataStore = new MemoryStorage();

                // Create Conversation State object.
                // The Conversation State object is where we persist anything at the conversation-scope.
                var conversationState = new ConversationState(dataStore);
                var userState = new UserState(dataStore);
                var privateConversationState = new PrivateConversationState(dataStore);

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                accessors = new BotAccessors(conversationState, privateConversationState, userState)
                {
                    UserData = userState.CreateProperty<BotDataBag>(BotAccessors.UserDataPropertyName),
                    ConversationData = conversationState.CreateProperty<BotDataBag>(BotAccessors.ConversationDataPropertyName),
                    PrivateConversationData = privateConversationState.CreateProperty<BotDataBag>(BotAccessors.PrivateConversationDataPropertyName),
                    DialogData = conversationState.CreateProperty<DialogState>(nameof(DialogState))
                };

            });

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterInstance(accessors).As<BotAccessors>();

            var container = builder.Build();

            var resolver = new AutofacWebApiDependencyResolver(container);
            config.DependencyResolver = resolver;
        }
    }
}
