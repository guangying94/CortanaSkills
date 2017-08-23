using Microsoft.Bot.Sample.LocalBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// Here, we are updating the database for feedback collection
// This is done via Azure Table Storage and Azure Function
// This function create the body of http request and perform http post

namespace SLADemo2.Dialog
{
    public class updateDatabase
    {
        private const string feedbackURL = "https://recordfeedback.azurewebsites.net/api/RecordFeedback?code=";
        private const string feedbackKey = "<Azure function API Key>";

        public static async Task<string> updateTables(double lat, double lon, string feedback, double sentiment, string email, string status)
        {
            using (var client = new HttpClient())
            {
                Random rand = new Random();
                int age = rand.Next(20, 60);
                client.BaseAddress = new Uri(feedbackURL + feedbackKey);
                byte[] byteData = Encoding.UTF8.GetBytes("{\"lat\":" + lat.ToString() + 
                    ",\"lon\":" + lon.ToString() + 
                    ",\"nric\": \"A1234567\"" + 
                    ",\"feedback\":\"" + feedback +
                    "\",\"sentiment\":" + sentiment.ToString() + 
                    ",\"age\":" + age.ToString() + 
                    ",\"gender\": \"male\"" + 
                    ",\"hp\": \"12345678\"" + 
                    ",\"email\":\"" + email + 
                    "\",\"area\": \"west\"" + 
                    ", \"status\":\"" + status + "\"}");

                var itemContent = new ByteArrayContent(byteData);
                itemContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(feedbackURL + feedbackKey, itemContent);
            }                
            return "ok";
        }
    }
}