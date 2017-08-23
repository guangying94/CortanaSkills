namespace Microsoft.Bot.Sample.LocalBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Builder.Dialogs;
    using Builder.Luis;
    using Builder.Luis.Models;
    using Newtonsoft.Json;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Net;
    using System.Text;
    using System.Collections;
    using Microsoft.Bot.Connector;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;

    // main conversation falls here
    // for demo purpose, this chatbot will have 5 main functions
    // check owned proterties (hard coded), get direction to buildings, contact support, get price for the land (hard coded), and faq mode
    // note that this chatbot is created for both usual messaging apps, as well as cortana for voice interaction
    // a.k.a. cortana skills

    [LuisModel("<LUIS App ID>", "<LUIS App PW>")]

    [Serializable]
    public class EngLuisDialog : LuisDialog<object>
    {
        // hard coded demo to construct feedback from "users"

        protected string channel = "";
        protected string loginOption = "";
        protected double lat = 1.349476;
        protected double lon = 103.756119;
        protected string feedback = "";
        protected double sentiment = 0;
        protected string email = "abc@xyz.com";

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> activity,LuisResult result)
        {
            // for debugging to test protactive messages
            var message = await activity;
            string conversationId = message.Conversation.Id;
            /*
            string toId = message.From.Id;
            string toName = message.From.Name;
            string fromId = message.Recipient.Id;
            string fromName = message.Recipient.Name;
            string serviceUrl = message.ServiceUrl;
            string channelId = message.ChannelId;
            
            await context.PostAsync(toId); //
            await context.PostAsync(toName);
            await context.PostAsync(fromId);
            await context.PostAsync(fromName);
            await context.PostAsync(serviceUrl);
            await context.PostAsync(channelId);
            */
            await context.PostAsync(conversationId);
            //await SLADemo2.Dialogs.ProactiveMessage.Resume("test", 0.5);
            context.Wait(MessageReceived);
        }

        // this intent will check the properties you owned
        // if the channel is via cortana, then it will assume that you have connected the account with cortana to access the info
        // else, you are prompted to login, via web, face or voice
        // for web, it's just a simple sign-in card, without any authentication
        // for face authentication, refer to "identifyFace.cs"
        // cognitive services speech api is able to differentiate the voice, work in progress

        [LuisIntent("checkProperty")]
        public async Task checkProperty(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var activity2 = await activity;
            string channelID = activity2.ChannelId;
            // here to retrieve the info of users via cortana
            //here you can retrieve information such as user name, saved location etc
            // here, i have saved my "home" location with a proper address

            if(channelID == "cortana")
            {
                if(activity2.Entities != null)
                {
                    var userInfo = activity2.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
                    var nameObj = userInfo.Properties["UserName"];
                    string userFamilyName = nameObj.Value<string>("FamilyName");
                    string userGivenName = nameObj.Value<string>("GivenName");
                    var frequentPlaces = userInfo.Properties["FrequentPlaces"];
                    string address = null; 
                    if(frequentPlaces != null)
                    {
                        int count = frequentPlaces.Count();
                        for(int i = 0; i < count; i++)
                        {
                            if(frequentPlaces[i].Value<string>("Type") == "Home")
                            {
                                address = frequentPlaces[i].Value<string>("Address");
                            }
                        }
                    }
                    // if address is detected, then it will ask you to confirm the address
                    if(address != null)
                    {
                        List<string> options = new List<string> { "Yes", "No" ,"Edit"};
                        string message = "#Information\n\n**Is the information correct?**\n\n**Name**: " + userFamilyName + " " + userGivenName + "\n\n**Address**: " + address;
                        var returnOption = new PromptOptions<string>(
                            message, retry: "Sorry I didn't get that", options: options, speak: "Is the following information correct?", retrySpeak: "Sorry I didn't get that");
                        PromptDialog.Choice(
                            context,
                            confirmInfoAsync,
                            returnOption
                        );
                    }
                    else
                    {
                        await context.SayAsync("No address detected.", "No address detected");
                        context.Wait(MessageReceived);
                    }
                }
            }
            // if the channel is not cortana, then it will go thru the usual method
            // i.e. options to authenticate users
            // for demo purpose, we will use api to authenticate
            // authentication part is hard coded, no OAuth is coded

            else
            {
                List<string> options = new List<string> { "Web Sign-in", "Face","Voice" };
                var returnOption = new PromptOptions<string>(
                    "First you need to sign in. How do you want to sign-in?", retry: "Sorry I didn't get that", options: options, speak: "How do you want to sign in?", retrySpeak: "Sorry I didn't get that");
                PromptDialog.Choice(
                    context,
                    signInAsync,
                    returnOption
                );
            }           
        }

        [LuisIntent("getDirection")]
        public async Task getDirection(IDialogContext context, IAwaitable<IMessageActivity> activity,LuisResult result)
        {
            // users can get the direction to SLA office 
            // if the channel is via cortana, it will launch map with the navigation directly
            var activity2 = await activity;
            string channelID = activity2.ChannelId;
            if(channelID == "cortana")
            {
                var response = context.MakeMessage();
                response.Text = "Launching maps for you.";
                response.Speak = "Launching maps.";
                await context.PostAsync(response);
                // via cortana, you can get the current location of users
                // then the navigation will direct from current location to SLA office
                if (activity2.Entities != null)
                {
                    var userInfo = activity2.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
                    if (userInfo != null)
                    {
                        string lat = "";
                        string lon = "";
                        var currentLocation = userInfo.Properties["CurrentLocation"];
                        if (currentLocation != null)
                        {
                            var hub = currentLocation["Hub"];
                            lat = hub.Value<double>("Latitude").ToString();
                            lon = hub.Value<double>("Longitude").ToString();
                        }
                        string uriAddress = "bingmaps:?rtp=pos." + lat + "_" + lon + "~pos.1.319570_103.841841";

                        var message = context.MakeMessage() as IMessageActivity;
                        message.ChannelData = JObject.FromObject(new
                        {
                            action = new { type = "LaunchUri", uri = uriAddress }
                        });
                        await context.PostAsync(message);
                    }
                    else
                    // if current location is not found
                    {
                        var message = context.MakeMessage() as IMessageActivity;
                        message.ChannelData = JObject.FromObject(new
                        {
                            action = new { type = "LaunchUri", uri = "bingmaps:?cp=1.319570~-103.841841&lvl=10" }
                        });
                        await context.PostAsync(message);
                    }
                }
            }
            else
            {
                // if it's not via cortana, it simply send an image with a short message
                var sendMap = context.MakeMessage();
                sendMap.Attachments = new List<Attachment>()
                {
                    new Attachment()
                    {
                        ContentUrl = "https://developers.onemap.sg/commonapi/staticmap/getStaticImage?layerchosen=default&lat=1.319552&lng=103.841743&zoom=17&height=300&width=300&polygons=&lines=&points=[1.319552,103.841743,\"175,50,0\",\"A\"]&color=&fillColor=",
                        ContentType = "image/png",
                        Name = "Map image"
                    }
                };
                sendMap.Text = "The address is 55 Newton Road, #12-01, Singapore 307987";
                await context.PostAsync(sendMap);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("callSupport")]
        public async Task callSupport(IDialogContext context, IAwaitable<IMessageActivity> activity ,LuisResult result)
        {
            // again, if it's via cortana skills, it will launch skype and contact a dummy account
            // else, it will just response the contact method
            var activity2 = await activity;
            string channelID = activity2.ChannelId;
            if(channelID == "cortana")
            {
                var response = context.MakeMessage();
                response.Text = "Thank you for contacting us, we are connecting you with customer service.";
                response.Speak = "Alright, let me help you contact the department.";
                await context.PostAsync(response);

                var message = context.MakeMessage() as IMessageActivity;
                message.ChannelData = JObject.FromObject(new
                {
                    action = new { type = "LaunchUri", uri = "skype:live:emsdemo999?call" }
                });
                await context.PostAsync(message);
            }
            else
            {
                await context.PostAsync("You can contact us at 12345678 for support.");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("getPrice")]
        public async Task getPrice(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            // similar to get direction, where we retrieve location of users via cortana
            // note that the response is hard coded, for demo purpose
            var activity2 = await activity;
            string channelID = activity2.ChannelId;

            if(channelID == "cortana")
            {
                if (activity2.Entities != null)
                {
                    var userInfo = activity2.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
                    if (userInfo != null)
                    {
                        var currentLocation = userInfo.Properties["CurrentLocation"];
                        double lat = 0;
                        double lon = 0;
                        if(currentLocation != null)
                        {
                            var hub = currentLocation["Hub"];
                            lat = hub.Value<double>("Latitude");
                            lon = hub.Value<double>("Longitude");
                        }
                        string mapURL = "https://developers.onemap.sg/commonapi/staticmap/getStaticImage?layerchosen=default&lat="+lat+"&lng="+ lon + "&zoom=17&height=300&width=300&polygons=&lines=&points=[" + lat + "," + lon + ",\"175,50,0\",\"A\"]&color=&fillColor=";
                        var reply = context.MakeMessage();
                        reply.Attachments.Add(new Attachment()
                        {
                            ContentUrl = mapURL,
                            ContentType = "image/png",
                            Name = "map.png"
                        });
                        reply.Speak = "Currently it's 60 Dollar per square meter per month.";
                        reply.Text = "The current price is S$60 per square meter per month as of Q1 2016 for this area.";
                        reply.InputHint = InputHints.AcceptingInput;
                        await context.PostAsync(reply);
                    }
                    else
                    {
                        var response = context.MakeMessage();
                        response.Text = "Sorry, I can't access your information right now.";
                        response.Speak = "Hey, I can't get any information now, try again later!";
                        await context.PostAsync(response);
                        context.Wait(MessageReceived);
                    }
                }
            }
            else
            {
                // response for other channel
                await context.PostAsync("work in progress");
                context.Wait(MessageReceived);
            }

        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello!"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("getAnswer")]
        public async Task getAnswer(IDialogContext context, IAwaitable<IMessageActivity> activity,LuisResult result)
        {
            // instead of entering qnamaker directly, it will only be triggered when users trigger this intent
            // then from there, it will be http request to qnamaker
            var activity2 = await activity;
            string channelID = activity2.ChannelId;
            this.channel = channelID;
            if(channel == "cortana")
            {
                var userInfo = activity2.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
                if (userInfo != null)
                {
                    var currentLocation = userInfo.Properties["CurrentLocation"];
                    if (currentLocation != null)
                    {
                        var hub = currentLocation["Hub"];
                        this.lat = hub.Value<double>("Latitude");
                        this.lon = hub.Value<double>("Longitude");
                    }
                }
            }
            await askQuestionAsync(context);
        }

        private async Task afterAddressAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            string status = await arguement;
            if(status == "Yes")
            {
                await context.SayAsync("The current price is $100 as of 30th of May 2017", "It's $100.");
            }
            else
            {
                await context.SayAsync("Okay, anything I can help?", "Alright, is there anything I can help?");
            }
        }

        private async Task askQuestionAsync(IDialogContext context)
        {
            //prompt to get questions
            var askOption = new PromptOptions<string>(
                            "Yes how can I help you today?", retry: "Try again", speak: "What questions do you have?", retrySpeak: "Speak clearly");
            var prompt = new PromptDialog.PromptString(askOption);
            context.Call<string>(prompt, afterQuestionAsync);
        }

        private async Task afterQuestionAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            // prompt to ask if users want to continue qna mode, or exit
            string queries = await arguement;
            string answer = await SLADemo2.Dialog.qnaMaker.GetFAQ(queries);
            List<string> options = new List<string> { "Thanks", "I still have questions" };
            var returnOption = new PromptOptions<string>(
                answer, retry: "Sorry I didn't get that", options: options, speak: answer, retrySpeak: "Sorry I didn't get that");
            PromptDialog.Choice(
                context,
                afterAnswerAsync,
                returnOption
            );
        }

        private async Task afterAnswerAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            string userResponse = await arguement;
            // users continue in qna mode
            if(userResponse.Contains("questions"))
            {
                await askQuestionAsync(context);
            }
            else
            {
                // users exit qna mode
                List<string> options = new List<string> { "Yes", "No" };
                var returnOption = new PromptOptions<string>(
                    "Can you tell us your feedback?", retry: "Sorry I didn't get that", options: options, speak: "Can you give us your feedback?", retrySpeak: "Sorry I didn't get that");
                PromptDialog.Choice(
                    context,
                    provideFeedbackAsync,
                    returnOption
                );
            }
        }

        private async Task provideFeedbackAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            // get permission on giving feedback
            string userResponse = await arguement;
            if(userResponse == "Yes")
            {
                var askOption = new PromptOptions<string>(
                            "What do you think about our new service?", retry: "Try again", speak: "How do feel with our service?", retrySpeak: "Speak clearly");
                var prompt = new PromptDialog.PromptString(askOption);
                context.Call<string>(prompt, afterFeedbackAsync);
            }
            else
            {
                await context.PostAsync("No worries, hope you enjoy our service!", "Hop you enjoy our service!");
            }
        }

        private async Task afterFeedbackAsync(IDialogContext context, IAwaitable<string> arguement)
        {           
            // get the permission and perform sentiment analysis to check if need to alert admin
            // if sentiment score is low, it will trigger an email and send an proactive message to teams group
            // also, all response will be recorded in table storage

            string Userfeedback = await arguement;
            this.feedback = Userfeedback;
            double sentimentScore = await SLADemo2.Dialog.SentimentAnalysis.MakeRequests(Userfeedback);
            this.sentiment = sentimentScore;
            if(sentimentScore > 0.6)
            {
                await SLADemo2.Dialog.updateDatabase.updateTables(lat, lon, feedback, sentiment, email, "closed");
                await context.SayAsync("Thank you for your feedback!", "Thanks for your feedback!");               
            }
            else
            {
                await SLADemo2.Dialogs.ProactiveMessage.Resume(feedback, sentiment);
                List<string> options = new List<string> { "Yes", "No" };
                var returnOption = new PromptOptions<string>(
                    "We are sorry to hear that. Do you want to contact support now?", retry: "Sorry I didn't get that", options: options, speak: "Oh no! Do you want to contact support now?", retrySpeak: "Sorry I didn't get that");
                PromptDialog.Choice(
                    context,
                    contactSupportAsync,
                    returnOption
                );
            }
        }

        private async Task contactSupportAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            // this will only be triggered if the sentiment is bad and users can to contact support
            // if it's via cortana channel, it will launch skype directly
            // else, it will just provide contact method

            string userResponse = await arguement;
            if(userResponse == "Yes")
            {
                if (this.channel == "cortana")
                {
                    await SLADemo2.Dialog.updateDatabase.updateTables(lat, lon, feedback, sentiment, email, "closed");
                    var response = context.MakeMessage();
                    response.Text = "Thank you for contacting us, we are connecting you with customer service.";
                    response.Speak = "Alright, let me help you contact the department.";
                    await context.PostAsync(response);

                    var message = context.MakeMessage() as IMessageActivity;
                    message.ChannelData = JObject.FromObject(new
                    {
                        action = new { type = "LaunchUri", uri = "skype:live:emsdemo999?call" }
                    });
                    await context.PostAsync(message);
                }
                else
                {
                    await SLADemo2.Dialog.updateDatabase.updateTables(lat, lon, feedback, sentiment, email, "open");
                    await context.PostAsync("I can't launch the phone app now. You can reach us @ 12345678.");
                    context.Wait(MessageReceived);
                }
            }
            else
            {
                if(this.channel == "cortana")
                {
                    await context.SayAsync("Thank you. You can contact us at 12345678", "No worries, this is our support number.");
                }
                else
                {
                    var askOption = new PromptOptions<string>(
            "Please provide us your email for further communication.", retry: "Try again", speak: "Please provide your email for further communication.", retrySpeak: "Speak clearly");
                    var prompt = new PromptDialog.PromptString(askOption);
                    context.Call<string>(prompt, afterEmailAsync);
                }
            }
        }

        private async Task afterEmailAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            this.email = await arguement;
            await SLADemo2.Dialog.updateDatabase.updateTables(lat, lon, feedback, sentiment, email, "open");
            await context.SayAsync("Thank you. You can contact us at 12345678", "No worries, this is our support number.");
        }

        private async Task confirmInfoAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            string userResponse = await arguement;
            if(userResponse == "Yes")
            {
                // no data available, hard code
                var message = context.MakeMessage() as IMessageActivity;
                message.Text = "According to HDB, the address that you provided is not registered under your name. It is a rental unit.";
                message.Speak = "You don't have any registered properties. I can only find rental unit under your name.";
                await context.PostAsync(message);
            }
            else
            {
                var message = context.MakeMessage() as IMessageActivity;
                message.Speak = "Can you click the link below to edit?";
                message.TextFormat = TextFormatTypes.Markdown;
                message.Summary = "Edit Particulars";
                message.Text = "To edit or update particulars, please click the link below.\n\n[MyProperty website](https://www.sla.gov.sg/MyProperty/#/home)";
                await context.PostAsync(message);
            } 
        }

        private async Task signInAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            // based on the selection, users need to input their NRIC to validate themselves
            // if user choose web sign-in, then they simply sign-in
            // note that no OAuth is coded, this is just for illustration purpose
            string userResponse = await arguement;
            switch (userResponse)
                {
                case "Web Sign-in":
                    var reply = context.MakeMessage();
                    reply.Type = "message";
                    reply.Attachments = new List<Attachment>();
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "https://www.singpass.gov.sg/spauth/login/loginpage?URL=/&TAM_OP=login",
                        Type = "signin",
                        Title = "Connect to Sing Pass"
                    };
                    cardButtons.Add(plButton);
                    SigninCard plCard = new SigninCard()
                    {
                        Text = "Login to Sing Pass",
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    reply.Attachments.Add(plAttachment);
                    await context.PostAsync(reply);
                    break;
                // apart from web, users need to provide their NRIC
                case "Face":
                    loginOption = "Face";
                    await askNRICAsync(context);
                    break;

                case "Voice":
                    loginOption = "Voice";
                    await askNRICAsync(context);
                    break;
            }
        }

        private async Task askNRICAsync(IDialogContext context)
        {
            // just to prompt to input nric
            PromptDialog.Text
                (
                context,
                afterNRICAsync,
                "What is your NRIC?"
                );
        }

        private async Task afterNRICAsync(IDialogContext context, IAwaitable<string> arguement)
        {
            // once nric is provided, we will check with table storage to verify if the nric is recorded
            // if yes, then only proceed with bro-authentication
            // if not, it will just prompt record is not found
            // note that voice recognition is worked in progress
            // for now, only face authentication works
            // once face is uploaded, it will trigger "faceReceivedAsync" function
            string NRIC = await arguement;
            NRIC = NRIC.ToUpper();
            string registered = await checkRegistrationAsync(NRIC);
            if(registered == "found")
            {
                if (loginOption == "Face")
                {
                    try
                    {
                        PromptDialog.Attachment(
                            context,
                            faceReceivedAsync,
                            "Please take a photo of your face."
                        );
                    }
                    catch (Exception)
                    {
                        await context.PostAsync("Photo of your face please");
                        context.Wait(MessageReceived);
                    }
                }
                else
                {
                    await context.PostAsync("Voice Recognition work in progress.");
                    context.Wait(MessageReceived);
                }
            }
            else
            {
                await context.PostAsync("Sorry, you are not registered in the system. Please contact us for support.");
                context.Wait(MessageReceived);
            }          
        }

        private async Task<string> checkRegistrationAsync(string nRIC)
        {
            // to check NRIC, we just check against the record in Azure table storage 
            // basically we perform http get against table storage to filter the results
            // if found, then proceed to next step
            string tableSig = "<Azure Table Storage Signature key>";
            string baseURL = "https://slademodatabase.table.core.windows.net:443/Identity?st=2017-06-03T07%3A21%3A00Z&se=2019-06-04T07%3A21%3A00Z&sp=raud&sv=2015-12-11&tn=identity&sig=";
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept", "application/json;odata=nometadata");
            string checkFilter = "&$filter=NRIC%20eq%20'" + nRIC + "'";
            string requestURL = baseURL + tableSig + checkFilter;
            var response = await http.GetAsync(requestURL);
            var result = await response.Content.ReadAsStringAsync();
            identityObj identityJSON = JsonConvert.DeserializeObject<identityObj>(result);
            if(identityJSON.value[0].NRIC == nRIC)
            {
                return "found";
            }
            else
            {
                return "error";
            }
        }

        private async Task faceReceivedAsync(IDialogContext context, IAwaitable<IEnumerable<Attachment>> arguement)
        {
            // here, the images will go thru "checkFace" to get the users
            // if face is not found, computer vision is performed to get the caption of this image
            // or else if face is found, then it will response the message
            // note that the message here is hard coded, purely for demo purpose

            var uploadImage = await arguement;
            if(uploadImage != null)
            {
                string faceURL = uploadImage.Last().ContentUrl;
                string userName = await SLADemo2.Dialog.FaceData.checkFace(faceURL);
                if(userName == "notFound")
                {
                    await context.PostAsync("Sorry, the face doesn't match with the registered face.");
                    context.Wait(MessageReceived);
                }
                else if(userName == "noFaceFound")
                {
                    visionObj visionJSON = await SLADemo2.Dialog.ComputerVision.GetImageJSON(faceURL);
                    string caption = visionJSON.description.captions[0].text;
                    await context.PostAsync("I think I saw " + caption + ". This is an invalid photo.");
                    context.Wait(MessageReceived);
                }
                else
                {
                    await context.PostAsync(userName + " is found.");
                    await context.PostAsync("According to HDB, the address that you provided is not registered under your name. It is a rental unit.");
                    context.Wait(MessageReceived);
                }
                
            }
            else
            {
                await context.PostAsync("No image received.");
            }
        }
    }
}