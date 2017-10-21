namespace Microsoft.Bot.ChinLuis
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

    //Work in progress, the bot will understand Chinese 
    [LuisModel("<LUIS App ID>", "<LUIS App PW>")]
    [Serializable]
    public class ChinLuisDialog : LuisDialog<object>
    {

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"我不明白你的意思。您说: {result.Query}"); 
            context.Wait(MessageReceived);
        }

        [LuisIntent("getPrice")]
        public async Task getPrice(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("一百块。"); 
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("你好!"); 
            context.Wait(MessageReceived);
        }

    }
}