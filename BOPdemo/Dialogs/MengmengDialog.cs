using BOPdemo.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace BOPdemo.Dialogs
{
    [LuisModel("c26cbbd3-4ee0-4487-a585-e9c17fd1ac40", "857c6cd0cf2744deb8f91b434379582d")]
    [Serializable]
    //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/c26cbbd3-4ee0-4487-a585-e9c17fd1ac40?
    //subscription-key=857c6cd0cf2744deb8f91b434379582d&timezoneOffset=0&verbose=true&q=
    public class MengmengDialog : LuisDialog<object>
    {
        public MengmengDialog()
        {
        }
        public MengmengDialog(ILuisService service)
        : base(service)
        {
        }
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            /**string message = $"小驰不知道你在说什么，面壁去。。。我现在只会查询天气。。T_T" + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);*/
            //var activity = await result as Activity;

            // calculate something for us to return
            //int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            //await context.PostAsync($"You sent {activity.Text} which was {length} characters");
            //await context.PostAsync("你好");

            /**
             * 发送
             **/
            string responseString = string.Empty;
            var query = result.Query; //User Query
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

            string answer = response.Answer;
            if (answer.Equals("No good match found in the KB"))
            {
                /**
                 * 如果匹配不到，调用图灵接口
                 */
                /**
                  * 发送
                  **/
                string responseString2 = string.Empty;
                var query2 = query; //User Query
                //Build the URI
                Uri qnamakerUriBase2 = new Uri("http://www.tuling123.com/openapi/api");
                var builder2 = new UriBuilder($"{qnamakerUriBase2}");

                //Add the question as part of the body
                //var postBody = $"{{\"question\": \"{query}\"}}";
                var postBody2 = $"{{\"key\":\"1add7afd90ee4f2fb1440cce6cf4cdda\", \"info\":\"{query2}\"}}";

                //Send the POST request
                using (WebClient client = new WebClient())
                {
                    //Set the encoding to UTF8
                    client.Encoding = System.Text.Encoding.UTF8;

                    //Add the subscription key header
                    client.Headers.Add("Content-Type", "application/json");
                    responseString2 = client.UploadString(builder2.Uri, postBody2);
                }
                Console.WriteLine(responseString2);
                /**
                 * 接收
                 **/
                //De-serialize the response
                TuLingResult tulingResult;
                try
                {
                    tulingResult = JsonConvert.DeserializeObject<TuLingResult>(responseString2);
                }
                catch
                {
                    throw new Exception("Unable to deserialize QnA Maker response string.");
                }

                string text = tulingResult.Text;
                await context.PostAsync(text);

                //await context.PostAsync("小驰不知道你在说什么，面壁去。。。我现在只会介绍微软俱乐部信息和查询天气。。T_T");
            }
            else
            {
                await context.PostAsync(answer);
            }
            //await context.PostAsync(response.Score+"");
            //原来的
            context.Wait(MessageReceived);
        }
        public bool TryToFindLocation(LuisResult result, out String location)
        {
            location = "";
            EntityRecommendation title;
            if (result.TryFindEntity("地点", out title))
            {
                location = title.Entity;
            }
            else
            {
                location = "";
            }
            return !location.Equals("");
        }
        [LuisIntent("查询天气")]
        public async Task QueryWeather(IDialogContext context, LuisResult result)
        {
            string location = "";
            string replyString = "";
            if (TryToFindLocation(result, out location))
            {
                replyString = await GetWeather(location);
                await context.PostAsync(replyString);
                context.Wait(MessageReceived);
            }
            else
            {
                await None(context, result);
            }
            //else
            //{
            //    await context.PostAsync("亲你要查询哪个地方的天气信息呢，快把城市的名字发给我吧");
            //context.Wait(AfterEnterLocation);
            //}
        }

        private async Task<string> GetWeather(string cityname)
        {
            WeatherData weatherdata = await BOPdemoTask.GetWeatherAsync(cityname);
            if (weatherdata == null || weatherdata.HeWeatherdataservice30 == null)
            {
                return string.Format("呃。。。萌萌不知道\"{0}\"这个城市的天气信息", cityname);
            }
            else
            {
                HeweatherDataService30[] weatherServices = weatherdata.HeWeatherdataservice30;
                if (weatherServices.Length <= 0) return string.Format("呃。。。萌萌不知道\"{0}\"这个城市的天气信息", cityname);
                Basic cityinfo = weatherServices[0].basic;
                if (cityinfo == null) return string.Format("呃。。。萌萌目测\"{0}\"这个应该不是一个城市的名字。。不然我咋不知道呢。。。", cityname);
                String cityinfoString = "城市信息：" + cityinfo.city + "\n\n"
                    + "更新时间：" + cityinfo.update.loc + "\n\n"
                    + "经纬度：" + cityinfo.lat + "," + cityinfo.lon + "\n\n";
                Aqi cityAirInfo = weatherServices[0].aqi;
                String airInfoString = "空气质量指数：" + cityAirInfo.city.aqi + "\n\n"
                    + "PM2.5 1小时平均值：" + cityAirInfo.city.pm25 + "(ug/m³)\n\n"
                    + "PM10 1小时平均值：" + cityAirInfo.city.pm10 + "(ug/m³)\n\n"
                    + "二氧化硫1小时平均值：" + cityAirInfo.city.so2 + "(ug/m³)\n\n"
                    + "二氧化氮1小时平均值：" + cityAirInfo.city.no2 + "(ug/m³)\n\n"
                    + "一氧化碳1小时平均值：" + cityAirInfo.city.co + "(ug/m³)\n\n";

                Suggestion citySuggestion = weatherServices[0].suggestion;
                String suggestionString = "生活指数：" + "\n\n"
                    + "穿衣指数：" + citySuggestion.drsg.txt + "\n\n"
                    + "紫外线指数：" + citySuggestion.uv.txt + "\n\n"
                    + "舒适度指数：" + citySuggestion.comf.txt + "\n\n"
                    + "旅游指数：" + citySuggestion.trav.txt + "\n\n"
                    + "感冒指数：" + citySuggestion.flu.txt + "\n\n";

                Daily_Forecast[] cityDailyForecast = weatherServices[0].daily_forecast;
                Now cityNowStatus = weatherServices[0].now;
                String nowStatusString = "天气实况：" + "\n\n"
                    + "当前温度(摄氏度)：" + cityNowStatus.tmp + "\n\n"
                    + "体感温度：" + cityNowStatus.fl + "\n\n"
                    + "风速：" + cityNowStatus.wind.spd + "(Kmph)\n\n"
                    + "湿度：" + cityNowStatus.hum + "(%)\n\n"
                    + "能见度：" + cityNowStatus.vis + "(km)\n\n";

                return string.Format("现在{0}天气实况：\n\n{1}", cityname, cityinfoString + nowStatusString + airInfoString + suggestionString);
            }
        }
    }
}