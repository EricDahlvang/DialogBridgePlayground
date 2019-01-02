using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class BridgeComponentDialog : ComponentDialog
    {
        public MessageReceivedAsync NextMethod { get; set; }
        
        public BridgeComponentDialog(string dialogId)
            : base(dialogId)
        {
        }

        public abstract Task StartAsync(DialogContext context);

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await StartAsync(outerDc);

            if (NextMethod != null)
            {
                outerDc.ActiveDialog.State["WaitMethod"] = NextMethod.Method.Name;

                //consume the next method that was just setup in StartAsync
                MessageReceivedAsync callNext = NextMethod;
                NextMethod = null;
                
                await callNext(outerDc, outerDc.Context.Activity.AsMessageActivity());
            }
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            //since we are continuing the dialog, consume the WaitMethod if present
            if (outerDc.ActiveDialog.State.ContainsKey("WaitMethod"))
            {
                ConsumeWaitMethod(outerDc);
            }
         
            //if resuming has setup another WaitMethod, then we can assume this dialog is in fact Waiting
            if (NextMethod != null)
            {
                outerDc.ActiveDialog.State["WaitMethod"] = NextMethod.Method.Name;
                return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting));
            }

            //also assume this dialog is waiting if it is still the active dialog
            var activeDialogId = outerDc.ActiveDialog.Id;
            if (FindDialog(activeDialogId) != null )
                return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting));
            else
                return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Complete));
        }

        public override Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (outerDc.ActiveDialog.State.ContainsKey("WaitMethod"))
            {
                ConsumeWaitMethod(outerDc);

                if (NextMethod != null)
                {
                    outerDc.ActiveDialog.State["WaitMethod"] = NextMethod.Method.Name;
                    return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting));
                }
                return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Complete));
            }   
            return base.ResumeDialogAsync(outerDc, reason, result, cancellationToken);
        }

        private void ConsumeWaitMethod(DialogContext outerDc)
        {
            //consume the wait method, and remove it so another one can be setup 
            var method = outerDc.ActiveDialog.State["WaitMethod"];
            //outerDc.ActiveDialog.State.Remove("WaitMethod");

            Type thisType = this.GetType();
            MethodInfo theMethod = thisType.GetMethod(method as string, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            theMethod.Invoke(this, new object[] { outerDc, outerDc.Context.Activity.AsMessageActivity() });
        }
    }
}