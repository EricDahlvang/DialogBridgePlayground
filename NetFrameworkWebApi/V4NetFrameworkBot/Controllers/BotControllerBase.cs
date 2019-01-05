using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using V3Migration;

namespace V4NetFrameworkBot.Controllers
{
    /// <summary>
    /// Adapted from https://github.com/Microsoft/BotBuilder-Samples/blob/master/samples/csharp_dotnetcore/30.asp-mvc-bot/BotControllerBase.cs
    /// </summary>
    public abstract class BotControllerBase : ApiController
    {
        protected BotAccessors Accessors { get; private set; }
        protected DialogSet Dialogs { get; private set; }

        public BotControllerBase(BotAccessors accessors)
        {
            this.Accessors = accessors;
            Dialogs = new DialogSet(accessors.DialogData);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostAsync([FromBody]Activity activity)
        {
            var botFrameworkAdapter = CreateAdapter();

            var invokeResponse = await botFrameworkAdapter.ProcessActivityAsync(
                Request.Headers.Authorization?.ToString(),
                activity,
                OnTurnAsync,
                default(CancellationToken));

            if (invokeResponse == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(invokeResponse.Status);
            }
        }

        /// <summary>
        /// Override CreateAdapter to create your own custom adapter or set properties on a BotFrameworkAdapter instance.
        /// </summary>
        /// <returns>An adapter that will be used for the current request.</returns>
        protected virtual IAdapterIntegration CreateAdapter() => new BotFrameworkAdapter(new SimpleCredentialProvider());

        /// <summary>
        /// Override OnTurnAsync to implement the bot specific logic. This is exactly the same as implementing
        /// the IBot OnTurnAsync.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        protected abstract Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken);
    }
}