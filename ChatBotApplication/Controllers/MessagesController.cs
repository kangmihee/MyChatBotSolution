using System;
using System.Linq;

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ChatBotApplication
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {


        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.BeautySalonRootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }





        private Activity HandleSystemMessage(Activity message)
        {
            string messageType = message.GetActivityType();
            if (messageType == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (messageType == ActivityTypes.ConversationUpdate)
            {
                //챗봇이 먼저 대화를 건다.
                /*if (message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                {
                    var reply = message.CreateReply("안녕하세요.\n\n 차홍아르더 도산점 예약 도우미 챗봇입니다.^^");
                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    connector.Conversations.ReplyToActivityAsync(reply);
                }*/
            }
            else if (messageType == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (messageType == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
            }
            else if (messageType == ActivityTypes.Ping)
            {
            }
            return null;
        }


    }
}