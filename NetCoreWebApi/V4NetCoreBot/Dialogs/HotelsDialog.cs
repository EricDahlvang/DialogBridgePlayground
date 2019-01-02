using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Bot.Builder.Community.Dialogs.FormFlow;

namespace V4NetCoreBot.Dialogs
{
    using System.Threading;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    [Serializable]
    public class HotelsQuery
    {
        [Prompt("Please enter your {&}")]
        public string Destination { get; set; }

        [Prompt("When do you want to {&}?")]
        public DateTime CheckIn { get; set; }

        [Numeric(1, int.MaxValue)]
        [Prompt("How many {&} do you want to stay?")]
        public int Nights { get; set; }
    }

    [Serializable]
    public class Hotel
    {
        public string Name { get; set; }

        public int Rating { get; set; }

        public int NumberOfReviews { get; set; }

        public int PriceStarting { get; set; }

        public string Image { get; set; }

        public string Location { get; set; }
    }

    [Serializable]
    public class HotelsDialog : BridgeComponentDialog
    {
        public HotelsDialog()
            : base(nameof(HotelsDialog))
        {
            var dialog = FormDialog.FromForm(this.BuildHotelsForm, FormOptions.PromptInStart);
            AddDialog(dialog);
        }

        public override async Task StartAsync(DialogContext context)
        {
            await context.PostAsync("Welcome to the Hotels finder!");

            var found = FindDialog("HotelsQuery");

            await found.BeginDialogAsync(context);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var found = FindDialog("HotelsQuery");

            var result = await found.ContinueDialogAsync(outerDc);
            if(result.Status == DialogTurnStatus.Complete)
            {
                await ResumeAfterHotelsFormDialog(outerDc, result.Result as HotelsQuery);
            }
            return result;
        }

        private IForm<HotelsQuery> BuildHotelsForm()
        {
            OnCompletionAsyncDelegate<HotelsQuery> processHotelsSearch = async (context, state) =>
            {
                await context.PostAsync($"Ok. Searching for Hotels in {state.Destination} from {state.CheckIn.ToString("MM/dd")} to {state.CheckIn.AddDays(state.Nights).ToString("MM/dd")}...");
            };

            return new FormBuilder<HotelsQuery>()
                .Field(nameof(HotelsQuery.Destination))
                .Message("Looking for hotels in {Destination}...")
                .AddRemainingFields()
                .OnCompletion(processHotelsSearch)
                .Build();
        }

        private async Task ResumeAfterHotelsFormDialog(DialogContext context, HotelsQuery result)
        {
            try
            {
                var searchQuery = result;

                var hotels = await this.GetHotelsAsync(searchQuery);

                await context.PostAsync($"I found in total {hotels.Count()} hotels for your dates:");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var hotel in hotels)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = hotel.Name,
                        Subtitle = $"{hotel.Rating} starts. {hotel.NumberOfReviews} reviews. From ${hotel.PriceStarting} per night.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = hotel.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation. Quitting from the HotelsDialog";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private async Task<IEnumerable<Hotel>> GetHotelsAsync(HotelsQuery searchQuery)
        {
            var hotels = new List<Hotel>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Hotel hotel = new Hotel()
                {
                    Name = $"{searchQuery.Destination} Hotel {i}",
                    Location = searchQuery.Destination,
                    Rating = random.Next(1, 5),
                    NumberOfReviews = random.Next(0, 5000),
                    PriceStarting = random.Next(80, 450),
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260"
                };

                hotels.Add(hotel);
            }

            hotels.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            return hotels;
        }
    }
}