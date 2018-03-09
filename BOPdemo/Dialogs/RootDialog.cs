using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using BOPdemo.Models;
using Newtonsoft.Json;

namespace BOPdemo.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            //await context.PostAsync($"You sent {activity.Text} which was {length} characters");
            //await context.PostAsync("你好");

            /**
             * 发送
             **/
            string responseString = string.Empty;
            var query = activity.Text; //User Query
            var knowledgebaseId = "a00256ee-9316-4476-9fcb-0f6f7e10f8b6"; // Use knowledge base id created.
            var qnamakerSubscriptionKey = "417cc12cef9a4b64835b84b657dc4d73"; //Use subscription key assigned to you.

            //Build the URI
            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v1.0");
            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");

            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{query}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                //Add the subscription key header
                client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                client.Headers.Add("Content-Type", "application/json");
                responseString = client.UploadString(builder.Uri, postBody);
            }
            /**
             * 接收
             **/
            //De-serialize the response
            QnAMakerResult response;
            try
            {
                response = JsonConvert.DeserializeObject<QnAMakerResult>(responseString);
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }

            if (response.Answer.Equals("No good match found in the KB"))
            {
                await context.PostAsync("小驰不知道你在说什么，面壁去。。。我现在只会介绍微软俱乐部信息和查询天气。。T_T");
            }
            else
            {
                await context.PostAsync(response.Answer);
            }
            //await context.PostAsync(response.Score+"");
            //原来的
            context.Wait(MessageReceivedAsync);
        }
    }

}