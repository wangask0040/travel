using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;

namespace server
{
    class Geom
    {
        public string type { get; set; }
        public double[] coordinates { get; set; }
        public void FillData(double longi, double lati)
        {
            type = "Point";
            coordinates = new double[] { longi, lati };
        }
    }

    class SendWeiboReq
    {
        public string content { get; set; }
        public string[] photo { get; set; }
        public double longi { get; set; }
        public double lati { get; set; }
        public string account { get; set; }
        public string address { get; set; }
        public string weather { get; set; }
    }

    class WeiboInfo
    {
        public string content { get; set; }
        public string[] photo { get; set; }
        public Geom geom { get; set; }
        public string account { get; set; }
        public string address { get; set; }
        public string weather { get; set; }
        public void FillData(SendWeiboReq info)
        {
            content = info.content;
            photo = info.photo;
            account = info.account;
            address = info.address;
            weather = info.weather;
            geom = new Geom();
            geom.FillData(info.longi, info.lati);
        }
    }

    class Storage : HttpSvrBase
    {
        private MongoClient m_client;
        private IMongoCollection<WeiboInfo> m_collection;

        public Storage()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["weibodb"].InnerText);
            m_collection = m_client.GetDatabase("db").GetCollection<WeiboInfo>("weibo");
        }

        public async void SaveWeibo(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp, WeiboInfo info)
        {
            Result r = new Result();

            try
            {
                var t = m_collection.InsertOneAsync(info);
                await t;
                r.result = (int)Result.ResultCode.RC_ok;
                r.msg = "发送成功！";

            }
            catch
            {
                r.result = (int)Result.ResultCode.RC_failed;
                r.msg = "发送失败！";
            }

            string json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        public override void Proc(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/sendwb":
                    string s = GetBody(req);
                    SendWeiboReq info = JsonConvert.DeserializeObject<SendWeiboReq>(s);
                    WeiboInfo weiboinfo = new WeiboInfo();
                    weiboinfo.FillData(info);
                    SaveWeibo(req, rsp, weiboinfo);
                    break;
                default:
                    break;
            }
        }
    }
}
