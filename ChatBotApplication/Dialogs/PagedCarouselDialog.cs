﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;


namespace ChatBotApplication.Dialogs
{
    [Serializable]
    public abstract class PagedCarouselDialog<T> : IDialog<T>
    {
        private int pageNumber = 1;
        private int pageSize = 5;
        public virtual string Prompt { get; }
        public abstract PagedCarouselCards GetCarouselCards(int pageNumber, int pageSize);
        public abstract Task ProcessMessageReceived(IDialogContext context, string message);

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(this.Prompt ?? "캐로셀");

            await this.ShowProducts(context);

            context.Wait(this.MessageReceivedAsync);
        }

        protected async Task ShowProducts(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            var productsResult = this.GetCarouselCards(this.pageNumber, this.pageSize);
            foreach (HeroCard productCard in productsResult.Cards)
            {
                reply.Attachments.Add(productCard.ToAttachment());
            }

            await context.PostAsync(reply);

            if (productsResult.TotalCount > this.pageNumber * this.pageSize)
            {
                await this.ShowMoreOptions(context);
            }
        }

        protected async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.Text.Equals("ShowMe", StringComparison.InvariantCultureIgnoreCase))
            {
                this.pageNumber++;
                await this.StartAsync(context);
            }
            else
            {
                await this.ProcessMessageReceived(context, message.Text);
            }
        }

        private async Task ShowMoreOptions(IDialogContext context)
        {
            var moreOptionsReply = context.MakeMessage();
            moreOptionsReply.Attachments = new List<Attachment>
            {
                    new HeroCard()
                {
                    Text = "MoreOptions",
                    Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.ImBack, "ShowMe", value: "ShowMe")
                    }
                }.ToAttachment()
            };

            await context.PostAsync(moreOptionsReply);
        }


    }

    public class PagedCarouselCards
    {
        public IEnumerable<HeroCard> Cards { get; set; }

        public int TotalCount { get; set; }
    }



}