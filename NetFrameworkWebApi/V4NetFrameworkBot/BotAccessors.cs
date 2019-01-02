using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace V4NetFrameworkBot
{
    public class BotAccessors
    {
        public ConversationState ConversationState;
        public PrivateConversationState PrivateConversationState;
        public UserState UserState;

        public BotAccessors(ConversationState conversationState, PrivateConversationState privateConversationState, UserState userState)
        {
            this.ConversationState = conversationState;
            this.PrivateConversationState = privateConversationState;
            this.UserState = userState;
        }

        public IStatePropertyAccessor<BotDataBag> UserData { get; set; }
        public IStatePropertyAccessor<BotDataBag> ConversationData { get; set; }
        public IStatePropertyAccessor<BotDataBag> PrivateConversationData { get; set; }
        public IStatePropertyAccessor<DialogState> DialogData { get; set; }

        public const string UserDataPropertyName = "UserData";
        public const string ConversationDataPropertyName = "ConversationData";
        public const string PrivateConversationDataPropertyName = "PrivateConversationData";
    }
}