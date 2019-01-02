﻿using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.FormFlow;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.AspNetCore.Mvc;
using V4NetCoreBot.Dialogs;

namespace V4NetCoreBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : BotControllerBase
    {
        public MessagesController(BotAccessors accessors)
            : base(accessors)
        {
            //todo: add dialogs
            Dialogs.Add(new RootDialog());
            Dialogs.Add(new HotelsDialog());
            Dialogs.Add(new SupportDialog());
        }

        protected async override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Text.Trim().Replace(" ", "").ToUpper() == "SHOWDATA")
                {
                    await ShowData(turnContext);
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
                    catch (FormCanceledException ex)
                    {
                        await turnContext.SendActivityAsync("Cancelled.");
                        await dc.CancelAllDialogsAsync();
                        await dc.BeginDialogAsync(nameof(RootDialog));
                    }
                }
            }
        }
        
        private static async Task ShowData(ITurnContext turnContext)
        {
            IBotDataBag userData = turnContext.UserData();
            string userDataValue = string.Empty;
            userData.TryGetValue("UserDataValue", out userDataValue);

            IBotDataBag conversationData = turnContext.ConversationData();
            string conversationDataValue = string.Empty;
            conversationData.TryGetValue("ConversationDataValue", out conversationDataValue);

            IBotDataBag privateConversationData = turnContext.PrivateConversationData();
            string privateConversationDataValue = string.Empty;
            privateConversationData.TryGetValue("PrivateConversationDataValue", out privateConversationDataValue);

            await turnContext.SendActivityAsync($"UserData.UserDataValue={userDataValue}");
            await turnContext.SendActivityAsync($"ConversationData.ConversationDataValue={conversationDataValue}");
            await turnContext.SendActivityAsync($"PrivateConversationData.PrivateConversationDataValue={privateConversationDataValue}");
        }
    }
}
