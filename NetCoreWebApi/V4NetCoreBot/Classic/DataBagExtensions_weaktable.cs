//using System.Runtime.CompilerServices;

//namespace Microsoft.Bot.Builder.Dialogs
//{
//    /// <summary>
//    /// Extensions for enabling DialogContext BotDataBag methods.
//    /// </summary>
//    public static class DataBagExtensions
//    {
//        private static readonly ConditionalWeakTable<ITurnContext, IBotDataBag> _userData = new ConditionalWeakTable<ITurnContext, IBotDataBag>();
//        private static readonly ConditionalWeakTable<ITurnContext, IBotDataBag> _privateConversationData = new ConditionalWeakTable<ITurnContext, IBotDataBag>();
//        private static readonly ConditionalWeakTable<ITurnContext, IBotDataBag> _conversationData = new ConditionalWeakTable<ITurnContext, IBotDataBag>();

//        public static IBotDataBag UserData(this DialogContext dialogContext)
//        {
//            return dialogContext.Context.UserData();
//        }

//        public static IBotDataBag PrivateConversationData(this DialogContext dialogContext)
//        {
//            return dialogContext.Context.PrivateConversationData();
//        }

//        public static IBotDataBag ConversationData(this DialogContext dialogContext)
//        {
//            return dialogContext.Context.ConversationData();
//        }

//        public static void UserData(this ITurnContext context, IBotDataBag dataBag)
//        {
//            _userData.Remove(context);
//            _userData.Add(context, dataBag);
//        }

//        public static IBotDataBag UserData(this ITurnContext context)
//        {
//            IBotDataBag bag;
//            if (_userData.TryGetValue(context, out bag))
//            {
//                return bag;
//            }

//            return new BotDataBag();
//        }

//        public static void PrivateConversationData(this ITurnContext context, IBotDataBag dataBag)
//        {
//            _privateConversationData.Remove(context);
//            _privateConversationData.Add(context, dataBag);
//        }

//        public static IBotDataBag PrivateConversationData(this ITurnContext context)
//        {
//            IBotDataBag bag;
//            if (_privateConversationData.TryGetValue(context, out bag))
//            {
//                return bag;
//            }

//            return new BotDataBag();
//        }
//        public static void ConversationData(this ITurnContext context, IBotDataBag dataBag)
//        {
//            _conversationData.Remove(context);
//            _conversationData.Add(context, dataBag);
//        }

//        public static IBotDataBag ConversationData(this ITurnContext context)
//        {
//            IBotDataBag bag;
//            if (_conversationData.TryGetValue(context, out bag))
//            {
//                return bag;
//            }

//            return new BotDataBag();
//        }

//        public static void RemoveWeakExtensions(this ITurnContext context)
//        {
//            _userData.Remove(context);
//            _privateConversationData.Remove(context);
//            _conversationData.Remove(context);
//        }        
//    }
//}