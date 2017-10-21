using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

// This code send proactive messages to Teams
// Proactive messages when the system received feedback
// by right, the information should be stored in database, but for demo purpose, the triggered users is only myself
// so the information is hard coded

namespace SLADemo2.Dialogs
{
    public class ProactiveMessage

    {
        public static string fromId = "<from ID>";
        public static string fromName = "<from Name>";
        public static string toId = "<to ID>";
        public static string toName = "<to Name>";
        public static string serviceUrl = "<service URL";
        public static string channelId = "msteams";
        public static string conversationId = "<conversation ID>";

        public static async Task Resume(string feedbackMsg, double sentiment)
        {
            // construct the connection for proactive messages
            var userAccount = new ChannelAccount(toId, toName);
            var botAccount = new ChannelAccount(fromId, fromName);
            var connector = new ConnectorClient(new Uri(serviceUrl));
            IMessageActivity message = Activity.CreateMessageActivity();
            if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(channelId))
            {
                message.ChannelId = channelId;
            }
            else
            {
                conversationId = (await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount)).Id;
            }
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: conversationId);
            // construct messages
            message.Text = "[Complaint received] \n\n" + feedbackMsg + " \n\n[Sentiment: " + Math.Round(sentiment, 2).ToString() + " ]";
            message.Locale = "en-Us";

            // send proactive messages
            await connector.Conversations.SendToConversationAsync((Activity)message);
        }
    }
}