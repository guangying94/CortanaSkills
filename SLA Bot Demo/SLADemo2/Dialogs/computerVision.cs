using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SLADemo2.Dialog
{
    public class ComputerVision
    {
        public static async Task<visionObj> GetImageJSON(string imageURL)
        {//Http post and retrive JSON from cognitive services
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "<computer vision api>");
            var uri = "https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Tags,Description&language=en";
            HttpResponseMessage response;
            byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + imageURL + "\"}");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                var result = await response.Content.ReadAsStringAsync();
                visionObj ImageJSON = JsonConvert.DeserializeObject<visionObj>(result);
                return ImageJSON;
            }
        }
    }
}