using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;
using V3Migration;

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
            BotAccessors accessors = null;

            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var credentialProvider = new SimpleCredentialProvider(ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppIdKey],
                                                                ConfigurationManager.AppSettings[MicrosoftAppCredentials.MicrosoftAppPasswordKey]);
            builder.RegisterInstance(credentialProvider).As<ICredentialProvider>();
            
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

            builder.RegisterInstance(accessors).As<BotAccessors>();
            var container = builder.Build();

            var resolver = new AutofacWebApiDependencyResolver(container);
            config.DependencyResolver = resolver;
        }
    }
}
