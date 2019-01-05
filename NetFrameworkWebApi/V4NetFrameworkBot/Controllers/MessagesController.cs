using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.FormFlow;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using V3Migration;
using V4NetFrameworkBot.Dialogs;

namespace V4NetFrameworkBot.Controllers
{
    public class MessagesController : BotControllerBase
    {
        ICredentialProvider _credentialProvider;
        public MessagesController(ICredentialProvider credentialProvider, BotAccessors accessors)
            : base(accessors)
        {
            _credentialProvider = credentialProvider;
            //todo: add dialogs
            Dialogs.Add(new RootDialog());
            Dialogs.Add(new HotelsDialog());
            Dialogs.Add(new SupportDialog());
        }

        protected override IAdapterIntegration CreateAdapter()
        {
            var adapter = new BotFrameworkAdapter(_credentialProvider, middleware: new DataBagsMiddleware(Accessors));
            adapter.Use(new AutoSaveStateMiddleware(Accessors.BotStates));
            return adapter;
        }

        protected override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Text.Trim().Replace(" ", "").ToUpper() == "SHOWDATA")
                {
                    await ShowData(turnContext);
                }
                else if (turnContext.Activity.Text.Contains(":"))
                {
                    SetData(turnContext);
                }
                else
                {
                    var dc = await Dialogs.CreateContextAsync(turnContext, cancellationToken);
                    try
                    {
                        var dialogResult = await dc.ContinueDialogAsync();

                        if (!dc.Context.Responded)
                        {
                            // examine results from active dialog
                            switch (dialogResult.Status)
                            {
                                case DialogTurnStatus.Empty:
                                    await dc.BeginDialogAsync(nameof(RootDialog));
                                    break;

                                case DialogTurnStatus.Waiting:
                                    // The active dialog is waiting for a response from the user, so do nothing.
                                    break;

                                case DialogTurnStatus.Complete:
                                    await dc.EndDialogAsync();
                                    break;

                                default:
                                    await dc.CancelAllDialogsAsync();
                                    break;
                            }
                        }
                    }
                    catch( FormCanceledException ex)
                    {
                        await turnContext.SendActivityAsync("Cancelled.");
                        await dc.CancelAllDialogsAsync();
                        await dc.BeginDialogAsync(nameof(RootDialog));
                    }
                    
                }
            }
        }

        private static void SetData(ITurnContext turnContext)
        {
            var setData = turnContext.Activity.Text.ToUpper().Split(':');
            if (setData[0].StartsWith("USER"))
            {
                turnContext.UserData().SetValue("UserDataValue", setData[1]);
            }
            else if (setData[0].StartsWith("PRIVATE"))
            {
                turnContext.PrivateConversationData().SetValue("PrivateConversationDataValue", setData[1]);
            }
            else
            {
                turnContext.ConversationData().SetValue("ConversationDataValue", setData[1]);
            }
        }

        private static async Task ShowData(ITurnContext turnContext)
        {
            IBotDataBag userData = turnContext.UserData();
            string userName = string.Empty;
            userData.TryGetValue("UserDataValue", out userName);

            IBotDataBag conversationData = turnContext.ConversationData();
            string conversationDataValue = string.Empty;
            conversationData.TryGetValue("ConversationDataValue", out conversationDataValue);

            IBotDataBag privateConversationData = turnContext.PrivateConversationData();
            string privateConversationDataValue = string.Empty;
            privateConversationData.TryGetValue("PrivateConversationDataValue", out privateConversationDataValue);
            
            await turnContext.SendActivityAsync($"UserData.UserName={userName}");
            await turnContext.SendActivityAsync($"ConversationData.ConversationDataValue={conversationDataValue}");
            await turnContext.SendActivityAsync($"PrivateConversationData.PrivateConversationDataValue={privateConversationDataValue}");
        }
    }
}
