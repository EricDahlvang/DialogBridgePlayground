namespace V4NetFrameworkBot.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;

    [Serializable]
    public class SupportDialog : BridgeComponentDialog
    {
        public SupportDialog()
            : base(nameof(SupportDialog)) { }

        public override async Task StartAsync(DialogContext context)
        {
            await context.PostAsync("what can i help you with");
            //context.Wait(this.MessageReceivedAsync);
        }
        public async override Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            await MessageReceivedAsync(outerDc, outerDc.Context.Activity);
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        public virtual async Task MessageReceivedAsync(DialogContext context, IMessageActivity result)
        {
            var message =  result;

            var ticketNumber = new Random().Next(0, 20000);

            await context.PostAsync($"Your message '{message.Text}' was registered. Once we resolve it; we will get back to you.");

            context.Done(ticketNumber);
        }
    }
}
