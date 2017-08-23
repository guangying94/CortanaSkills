using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// When users enter FAQ mode, i.e. asked chatbot "I have questions"
// this leverage on qnamaker.ai
// here, this function take in queries and perform http request

namespace SLADemo2.Dialog
{
    public class qnaMaker
    {
        private const string qnamakerSubscriptionKey = "<QnA Maker API Key>";

        public static async Task<string> GetFAQ(string queries)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
            var uri = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/d46900ca-9f12-4f2b-8707-8cfea09f47fc/generateAnswer";
            HttpResponseMessage response;
            byte[] byteData = Encoding.UTF8.GetBytes("{\"question\":\"" + queries + "\"}");
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                var result = await response.Content.ReadAsStringAsync();
                qnaObj qnaJSON = JsonConvert.DeserializeObject<qnaObj>(result);
                return qnaJSON.answers[0].answer;
            }
        }
    }
}