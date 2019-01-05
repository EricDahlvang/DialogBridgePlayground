using System.Runtime.CompilerServices;
using V3Migration;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Extensions for enabling DialogContext BotDataBag methods.
    /// </summary>
    public static class DataBagExtensions
    {
        public static IBotDataBag UserData(this DialogContext dialogContext)
        {
            return dialogContext.Context.UserData();
        }

        public static IBotDataBag PrivateConversationData(this DialogContext dialogContext)
        {
            return dialogContext.Context.PrivateConversationData();
        }

        public static IBotDataBag ConversationData(this DialogContext dialogContext)
        {
            return dialogContext.Context.ConversationData();
        }

        public static IBotDataBag UserData(this ITurnContext context)
        {
            object bag;
            if (context.TurnState.TryGetValue(BotAccessors.UserDataPropertyName, out bag))
            {
                return bag as IBotDataBag;
            }

            return new BotDataBag();
        }

        public static IBotDataBag PrivateConversationData(this ITurnContext context)
        {
            object bag;
            if (context.TurnState.TryGetValue(BotAccessors.PrivateConversationDataPropertyName, out bag))
            {
                return bag as IBotDataBag;
            }
            
            return new BotDataBag();
        }
        
        public static IBotDataBag ConversationData(this ITurnContext context)
        {
            object bag;
            if (context.TurnState.TryGetValue(BotAccessors.ConversationDataPropertyName, out bag))
            {
                return bag as IBotDataBag;
            }

            return new BotDataBag();
        }    
    }
}