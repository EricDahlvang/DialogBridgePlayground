using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace V3Migration
{
    public class DataBagsMiddleware : IMiddleware
    {
        BotAccessors _accessors;
        public DataBagsMiddleware(BotAccessors accessors)
        {
            _accessors = accessors;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            //add DataBags to TurnState so they will be available in DataBagExtensions
            turnContext.TurnState.Add(BotAccessors.UserDataPropertyName, await _accessors.UserData.GetAsync(turnContext, () => new BotDataBag()));
            turnContext.TurnState.Add(BotAccessors.ConversationDataPropertyName, await _accessors.ConversationData.GetAsync(turnContext, () => new BotDataBag()));
            turnContext.TurnState.Add(BotAccessors.PrivateConversationDataPropertyName, await _accessors.PrivateConversationData.GetAsync(turnContext, () => new BotDataBag()));

            await next(cancellationToken);
        }
    }
}
