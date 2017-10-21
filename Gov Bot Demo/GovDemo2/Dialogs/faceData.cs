using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// These functions leverage on cognitive services face api to recognize face
// here, you need to create a group id, then create a person id inside this group
// once done, upload the images in this person group
// face api will provide a unique face id tie to this person
// the matching is done via face id

namespace SLADemo2.Dialog
{
    public class FaceData
    {
        private const string groupId = "<face group id>";
        private const string MarcuspersonId = "<person id>";
        private const string MarcusFaceId = "<face id>";
        private const string apiKey = "<Face API Key>"; 

        // this function takes in the url of images sent by users
        // first, using "getFaceID" function to retrieve the unique face ID
        // if face is detected, i.e. the return value is not "noFace", then it will try to recognize who is it
        public static async Task<string> checkFace(string faceURL)
        {
            string face2 = await getFaceID(faceURL);

            if (face2 != "noFace")
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "apiKey");
                var uri = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0/identify?";
                HttpResponseMessage response;
                byte[] byteData = Encoding.UTF8.GetBytes("{\"personGroupId\":\"" + groupId + "\", \"faceIds\":[\"" + face2 + "\"],\"maxNumofCandidatesReturned\":1,\"ConfidenceThreshold\":0.6}");
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                    var result = await response.Content.ReadAsStringAsync();
                    var identifyFaceJSON = JsonConvert.DeserializeObject<identifyFaceObject[]>(result);
                    // if no identified face found via face api
                    if (identifyFaceJSON[0].candidates.Count < 1)
                    {
                        return "notFound";
                    }
                    else
                    {
                        // if recommended face is found, it will do matching here
                        string identifyPersonID = identifyFaceJSON[0].candidates[0].personId;
                        if (identifyPersonID == MarcuspersonId)
                        {
                            return "Tee Guang Ying";
                        }
                        else
                        {
                            return "Not found";
                        }
                    }
                }
            }
            else
            {
                return "noFaceFound";
            }
        }

        // when you send an image to cognitive services, it will return a unique face id
        // this function will retrive the face id for comparison
        // if the return response is empty or null, this function return "noFace"
        // else it will return the unique face id

        public static async Task<string> getFaceID(string faceURL)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "apiKey");
            var uri = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=\"true\"";
            HttpResponseMessage response;
            byte[] byteData = Encoding.UTF8.GetBytes("{\"url\":\"" + faceURL + "\"}");
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                var result = await response.Content.ReadAsStringAsync();
                if (result.Length < 5)
                {
                    return "noFace";
                }
                else
                {
                    var faceJSON = JsonConvert.DeserializeObject<faceObject[]>(result);
                    return faceJSON[0].faceId;
                }
            }
        }

        // Optional, can retrive the name directly from face API
        // for simplicity, the comparision / retrieve name is hard coded
        
        public static async Task<string> getName(string personID)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "apiKey");
            var uri = "https://westus.api.cognitive.microsoft.com/face/v1.0/persongroups/" + groupId + "/persons/" + personID;
            var response = await client.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();
            PersonObject personJSON = JsonConvert.DeserializeObject<PersonObject>(result);
            return personJSON.name;
        }
    }
}