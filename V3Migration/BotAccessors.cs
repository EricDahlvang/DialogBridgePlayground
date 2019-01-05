using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace V3Migration
{
    public class BotAccessors
    {
        public const string UserDataPropertyName = "UserData";
        public const string ConversationDataPropertyName = "ConversationData";
        public const string PrivateConversationDataPropertyName = "PrivateConversationData";

        public BotAccessors(ConversationState conversationState, PrivateConversationState privateConversationState, UserState userState)
        {
            this.ConversationState = conversationState;
            this.PrivateConversationState = privateConversationState;
            this.UserState = userState;

            BotStates = new BotState[] { ConversationState, PrivateConversationState, UserState };
        }

        public BotState[] BotStates { get; }

        public ConversationState ConversationState { get; }
        public PrivateConversationState PrivateConversationState { get; }
        public UserState UserState { get; }

        public IStatePropertyAccessor<BotDataBag> UserData { get; set; }
        public IStatePropertyAccessor<BotDataBag> ConversationData { get; set; }
        public IStatePropertyAccessor<BotDataBag> PrivateConversationData { get; set; }
        public IStatePropertyAccessor<DialogState> DialogData { get; set; }
    }
}
