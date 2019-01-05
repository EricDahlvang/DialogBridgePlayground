# V3-V4 migration ideas and concepts

## Purpose

Assist developers with migrating dotnet Bot Builder V3 projects to dotnet Bot Builder V4 by providing some examples and a high level overview of the differences and similarities.

## V3 Overview

When converting an existing bot from Bot Builder V3 to Bot Builder V4, it is helpful to know the differences and similarities between the versions.  For instance, within V3 the dialog stack is mostly managed internally.  Developers use IDialogContext.Call or IDialogContext.Forward to push dialogs onto the stack. StartAsync is the initialization method for dialogs and is called automatically by the sdk.  In V3, IDialogContext.Wait and ResumeAfter delegates are used to dynamically set the continuation for which method should process the next message received from the user.  IDialogContext is created by the sdk, and passed to StartAsync and the continuation methods.  Three DataBags (UserData, ConversationData, PrivateConversationData) are auto-populated and available on IDialogContext. In V3 dialogs fields are auto-serailized into PrivateConversationData, and re-populated when the dialog is next loaded.

## V3 IDialogContext:

interface IDialogContext : IDialogStack, IBotContext, IBotData, IBotToUser

IDialogStack:

        IReadOnlyList<Delegate> Frames { get; }

        void Call<R>(IDialog<R> child, ResumeAfter<R> resume);

        void Done<R>(R value);

        void Fail(Exception error);

        Task Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token);

        void Post<E>(E @event, ResumeAfter<E> resume);

        void Reset();

        void Wait<R>(ResumeAfter<R> resume);
		
IBotContext:

        CancellationToken CancellationToken { get; }

        IActivity Activity { get; }
		
IBotData:

        IBotDataBag UserData { get; }
        
        IBotDataBag ConversationData { get; }
        
        IBotDataBag PrivateConversationData { get; }

        Task FlushAsync(CancellationToken cancellationToken);
        
        Task LoadAsync(CancellationToken cancellationToken);
		
IBotToUser:
   
        IMessageActivity MakeMessage();
        
        Task PostAsync(IMessageActivity message, CancellationToken cancellationToken = default(CancellationToken));
    


## V4 Overview

Bot Builder dotnet V4 is more modularized than its predecessor. In fact, DialogContext is in an entirely different Nuget package than the core of the SDK: [Microsoft.Bot.Builder.Dialogs](https://www.nuget.org/packages/Microsoft.Bot.Builder.Dialogs/)  The dialog stack and state setup is more in the hands of the developer. Within the BeginDialogAsync and ContinueDialogAsync methods, the DialogContext has a BeginDialogAsync method that will start that dialog as a child of the current dialog (similar to .Forward in V3). Resuming after a dialog completes, and utilizing the value returned from the child dialog, is different and involves using DialogTurnResult.Result

DialogContext has references to the TurnContext and DialogInstance in V4 is the dialogContext.ActiveDialog. From the DialogInstance developers can retrieve the DialogState object, which is an IDictionary<string, object> implementation that is json serialized into/out of the store configured in Startup.cs.  Bot Builder V4 retains the concepts of UserData, ConversationData and PivateConversationData. Setup is a little different, but these data buckets are still scoped as they were:

- The user state creates a key using the channel ID and from ID. {Activity.ChannelId}/users/{Activity.From.Id}#YourPropertyName

- The conversation state creates a key using the channel ID and the conversation ID. {Activity.ChannelId}/conversations/{Activity.Conversation.Id}#YourPropertyName

- The private conversation state creates a key using the channel ID, from ID and the conversation ID. {Activity.ChannelId}/conversations/{Activity.Conversation.Id}/users/{Activity.From.Id}#YourPropertyName

State accessors are configured in Startup's ConfigureServices method. 


## V4 DialogContext:

DialogContext:

        public DialogSet Dialogs { get; }
        
        public ITurnContext Context { get; }
        
	public List<DialogInstance> Stack { get; }
        
        public DialogInstance ActiveDialog { get; }

        public Task<DialogTurnResult> BeginDialogAsync(string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken));
        
        public Task<DialogTurnResult> CancelAllDialogsAsync(CancellationToken cancellationToken = default(CancellationToken));
        
        public Task<DialogTurnResult> ContinueDialogAsync(CancellationToken cancellationToken = default(CancellationToken));
        
        public Task<DialogTurnResult> EndDialogAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken));
        
        public Task<DialogTurnResult> PromptAsync(string dialogId, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken));
        
        public Task<DialogTurnResult> ReplaceDialogAsync(string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken));
        
        public Task RepromptDialogAsync(CancellationToken cancellationToken = default(CancellationToken));
		
ITurnContext:

        BotAdapter Adapter { get; }
        
        TurnContextStateCollection TurnState { get; }
        
        Activity Activity { get; }
        
        bool Responded { get; }

        Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken));
        
        Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken));
        
        ITurnContext OnDeleteActivity(DeleteActivityHandler handler);
        
        ITurnContext OnSendActivities(SendActivitiesHandler handler);
        
        ITurnContext OnUpdateActivity(UpdateActivityHandler handler);
        
        Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken));
        
        Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default(CancellationToken));
        
        Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken));
        
        Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken));

# V3-V4 Migration Methods

## Manual Re-Write using Microsoft.Bot.Builder.Integration

The [Microsoft.Bot.Builder.Integration.AspNet.WebApi](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries/integration/Microsoft.Bot.Builder.Integration.AspNet.WebApi) and [Microsoft.Bot.Builder.Integration.AspNet.Core](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries/integration/Microsoft.Bot.Builder.Integration.AspNet.Core) libraries provide support for V4 bots.  These libraries use HttpClient Message Handlers that call into your IBot's OnTurn method.  Samples can be here: [csharp_webapi](https://github.com/Microsoft/BotBuilder-Samples/tree/master/samples/csharp_webapi) and [csharp_dotnetcore](https://github.com/Microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore) 

## Create Adapter in WebAPI Controller 

Migration Guidelines:


- upgrade Microsoft.Bot.Builder and Microsoft.Bot.Builder.Azure packages to 4.2.0

- add Microsoft.Bot.Builder.Dialogs

- Remove 'using' references to the following: (these no longer exist)
	
        using Microsoft.Bot.Builder.Internals.Fibers;

        using Microsoft.Bot.Builder.Dialogs.Internals;
        
        using Microsoft.Bot.Builder.Scorables;
	
        using Microsoft.Bot.Builder.Autofac.Base;

- string replace "IDialogContext" with "DialogContext"
	
- Remove Scorables which are not in V4.  Global interruptions are done differently now (within OnTurn itself)
	
- add "using Microsoft.Bot.Schema;" to files where it is needed
	
- Remove references to ActivityTypes.Ping this activity type no longer exists
