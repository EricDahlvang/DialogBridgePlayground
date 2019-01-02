namespace V4NetFrameworkBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    
    [Serializable]
    public class FlightsDialog : BridgeComponentDialog
    {
        public FlightsDialog()
            : base(nameof(FlightsDialog)) { }

        public override async Task StartAsync(DialogContext context)
        {         
            context.Fail(new NotImplementedException("Flights Dialog is not implemented and is instead being used to show context.Fail"));
        }
    }
}