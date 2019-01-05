using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public delegate Task MessageReceivedAsync(DialogContext context, IMessageActivity message);
    public delegate Task ResumeAfter(DialogContext context, IMessageActivity result);

    /// <summary>
    /// Extensions providing support for methods existing in Bot Builder V3 but not V4
    /// </summary>
    public static class ClassicExtensions
    {
        public static async Task PostAsync(this DialogContext dc, string message)
        {
            await dc.Context.SendActivityAsync(message).ConfigureAwait(false);
        }

        public static async Task PostAsync(this DialogContext dc, IMessageActivity message)
        {
            await dc.Context.SendActivityAsync(message).ConfigureAwait(false);
        }

        public static void Done(this DialogContext dc, object result)
        {
            dc.EndDialogAsync(result).Wait();
        }

        public static void Done<T>(this DialogContext dc, object result)
        {
           // dc.EndDialogAsync(result);

            var results = Task.Run(() => dc.EndDialogAsync(result)).Result;

            // dc.EndDialogAsync(result).Wait();
        }

        public static void Wait(this DialogContext dc, MessageReceivedAsync method)
        {
            var dialog = dc.Dialogs.Find(dc.ActiveDialog.Id);

            if (dialog != null)
            {
                var asV3 = dialog as BridgeComponentDialog;
                if (asV3 != null)
                {
                    if (asV3.NextMethod != null)
                    {
                        throw new InvalidOperationException("Dialog .NextMethod was not executed before the attempt to set it. (.Wait with missing execution)");
                    }
                    asV3.NextMethod = method;

                    dc.ActiveDialog.State["WaitMethod"] = method.Method.Name;
                }
            }
        }

        public static void Call<T>(this DialogContext dc, ComponentDialog dialog, ResumeAfter resumeAfter)
        {
            var foundChild = dc.Dialogs.Find(dialog.Id);
            if (foundChild == null)
            {
                dc.Dialogs.Add(dialog);
            }
            
            var instance = new DialogInstance() { Id = dialog.Id, State = new Dictionary<string, object>() };
            dc.Stack.Insert(0, instance);
            var result = Task.Run(() => dialog.BeginDialogAsync(dc)).Result;
        }

        public static async Task Forward<T>(this DialogContext dc, ComponentDialog dialog, ResumeAfter resumeAfter, IMessageActivity message, CancellationToken cancelationToken)
        {
            var foundChild = dc.Dialogs.Find(dialog.Id);
            if (foundChild == null)
            {
                dc.Dialogs.Add(dialog);
            }

            //ensure the current Dialog has the .ResumeAfter set correctly
            var asBridge = dc.Dialogs.Find(dc.ActiveDialog.Id) as BridgeComponentDialog;
            if (asBridge != null)
            {
                dc.ActiveDialog.State["WaitMethod"] = resumeAfter.Method.Name;
                //asBridge.ResumeAfter = resumeAfter;
            }
            
            var instance = new DialogInstance() { Id = dialog.Id, State = new Dictionary<string, object>() };
            dc.Stack.Insert(0, instance);


            var result = Task.Run(() => dialog.BeginDialogAsync(dc)).Result;
        }

        public static void Fail(this DialogContext dc, Exception ex)
        {
            dc.PostAsync(ex.Message).Wait();
            dc.CancelAllDialogsAsync().Wait();
        }

        public static T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (T)instance;
        }
    }
}