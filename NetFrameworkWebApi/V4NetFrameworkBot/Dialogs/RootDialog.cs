namespace V4NetFrameworkBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    [Serializable]
    public class RootDialog : BridgeComponentDialog
    {
        public RootDialog()
            : base(nameof(RootDialog))
        {
            AddDialog(new ChoicePrompt("options"));
            AddDialog(new HotelsDialog());
            AddDialog(new SupportDialog());
        }

        private const string FlightsOption = "Flights";

        private const string HotelsOption = "Hotels";

        public override async Task StartAsync(DialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        //public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    return base.ContinueDialogAsync(outerDc, cancellationToken);
        //}

        //public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    return base.ResumeDialogAsync(outerDc, reason, result, cancellationToken);
        //}

        public virtual async Task MessageReceivedAsync(DialogContext context, IMessageActivity result)
        {
            var message = result;

            if (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem"))
            {
                await context.Forward<int>(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else
            {
                await this.ShowOptions(context);
            }
        }

        private async Task ShowOptions(DialogContext context)
        {
            var options = new PromptOptions()
            {
                Choices = new List<Microsoft.Bot.Builder.Dialogs.Choices.Choice>() { new Microsoft.Bot.Builder.Dialogs.Choices.Choice(HotelsOption), new Microsoft.Bot.Builder.Dialogs.Choices.Choice(FlightsOption) },
                Prompt = MessageFactory.Text("Are you looking for a flight or a hotel?"),
                RetryPrompt = MessageFactory.Text("Not a valid option")
            };

            var optionsDialog = this.FindDialog("options");

            context.Wait(OnOptionSelected);
            await optionsDialog.BeginDialogAsync(context, options);


            //PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { FlightsOption, HotelsOption }, "Are you looking for a flight or a hotel?", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(DialogContext context, IMessageActivity result)
        {
            try
            {
                if (result.Text.ToLower().Contains("help") || result.Text.ToLower().Contains("support") || result.Text.ToLower().Contains("problem"))
                {
                    await context.Forward<int>(new SupportDialog(), this.ResumeAfterSupportDialog, result, CancellationToken.None);
                    return;
                }

                var optionSelected = result;

                switch (optionSelected.Text)
                {
                    case FlightsOption:
                        context.Call<object>(new FlightsDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case HotelsOption:
                        context.Call<object>(new HotelsDialog(), this.ResumeAfterOptionDialog);
                        break;

                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(DialogContext context, IMessageActivity result)
        {
            var ticketNumber = result;

            await context.PostAsync($"Thanks for contacting our support team. Your ticket number is {ticketNumber.Text}.");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterOptionDialog(DialogContext context, object result)
        {
            try
            {
                var message = result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}
