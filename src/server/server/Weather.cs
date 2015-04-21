using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace server
{
    class Weather
    {
        public async Task<string> GetWeather(string address)
        {
            try
            {
                HttpClient client = new HttpClient();
                string reqstr = string.Format(url, address);
                HttpResponseMessage response = await client.GetAsync(reqstr);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            string str = "zxc";
            return str;
        }

        private string url = "http://api.map.baidu.com/telematics/v3/weather?location={0}&output=json&ak=YdrtIvuTE25sNvAmxS05DgEo";
       
    }
}

/*
{"error":0,"status":"success","date":"2015-04-21","results":[{"currentCity":"深圳","pm25":"48","index":[{"title":"穿衣","zs":"较舒适","tipt":"穿衣指数","des":"建议着薄外套、开衫牛仔衫裤等服装。年老体弱者应适当添加衣物，宜着夹克衫、薄毛衣等。"},{"title":"洗车","zs":"较适宜","tipt":"洗车指数","des":"较适宜洗车，未来一天无雨，风力较小，擦洗一新的汽车至少能保持一天。"},{"title":"旅游","zs":"适宜","tipt":"旅游指数","des":"天气较好，温度适宜，总体来说还是好天气哦，这样的天气适宜旅游，您可以尽情地享受大自然的风光。"},{"title":"感冒","zs":"易发","tipt":"感冒指数","des":"相对于今天将会出现大幅度降温，易发生感冒，请注意适当增加衣服，加强自我防护避免感冒。"},{"title":"运动","zs":"较适宜","tipt":"运动指数","des":"阴天，较适宜进行各种户内外运动。"},{"title":"紫外线强度","zs":"最弱","tipt":"紫外线强度指数","des":"属弱紫外线辐射天气，无需特别防护。若长期在户外，建议涂擦SPF在8-12之间的防晒护肤品。"}],"weather_data":[{"date":"周二 04月21日 (实时：25℃)","dayPictureUrl":"http://api.map.baidu.com/images/weather/day/yin.png","nightPictureUrl":"http://api.map.baidu.com/images/weather/night/yin.png","weather":"阴","wind":"微风","temperature":"24 ~ 20℃"},{"date":"周三","dayPictureUrl":"http://api.map.baidu.com/images/weather/day/yin.png","nightPictureUrl":"http://api.map.baidu.com/images/weather/night/yin.png","weather":"阴","wind":"微风","temperature":"25 ~ 20℃"},{"date":"周四","dayPictureUrl":"http://api.map.baidu.com/images/weather/day/zhenyu.png","nightPictureUrl":"http://api.map.baidu.com/images/weather/night/zhenyu.png","weather":"阵雨","wind":"微风","temperature":"25 ~ 20℃"},{"date":"周五","dayPictureUrl":"http://api.map.baidu.com/images/weather/day/duoyun.png","nightPictureUrl":"http://api.map.baidu.com/images/weather/night/duoyun.png","weather":"多云","wind":"微风","temperature":"26 ~ 21℃"}]}]}
 */
