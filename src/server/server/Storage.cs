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
        public long AccountId { get; set; }
        public string address { get; set; }
        public string weather { get; set; }
    }

    class SendWeiboRsp : Result
    {
        public ObjectId _id { get; set; }
        public DateTime time { get; set; }
    }

    class WeiboInfo
    {
        public string content { get; set; }
        public string[] photo { get; set; }
        public Geom geom { get; set; }
        public long AccountId { get; set; }
        public string address { get; set; }
        public string weather { get; set; }
        public DateTime time { get; set; }
        public void FillData(SendWeiboReq info)
        {
            content = info.content;
            photo = info.photo;
            AccountId = info.AccountId;
            address = info.address;
            weather = info.weather;
            geom = new Geom();
            geom.FillData(info.longi, info.lati);
            time = new DateTime();
            TimeSpan ts = new TimeSpan(8, 0, 0);
            time = DateTime.Now + ts;

        }
    }

    class Storage : HttpSvrBase
    {
        private MongoClient m_client;
        private IMongoCollection<WeiboInfo> m_collection;

        private MongoClient m_findClient;
        private IMongoCollection<BsonDocument> m_findCollection;

        public Storage()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["weibodb"].InnerText);
            m_collection = m_client.GetDatabase("db").GetCollection<WeiboInfo>("weibo");

            m_findClient = new MongoClient(c.Root["weibodb"].InnerText);
            m_findCollection = m_client.GetDatabase("db").GetCollection<BsonDocument>("weibo");
        }

        public async void SaveWeibo(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp, WeiboInfo info)
        {
            SendWeiboRsp r = new SendWeiboRsp();

            try
            {
                //插入一条
                var t = m_collection.InsertOneAsync(info);
                await t;

                //查找
                var filter = Builders<BsonDocument>.Filter.Eq("time", info.time)
                    & Builders<BsonDocument>.Filter.Eq("AccountId", info.AccountId);

                var fopt = new FindOptions<BsonDocument>();
                fopt.Limit = 1;

                var f = m_findCollection.FindAsync(filter, fopt);
                
                using (var cursor = await f)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            r._id = new ObjectId();
                            r._id = document["_id"].AsObjectId;
                            r.time = document["time"].ToUniversalTime();
                            break;
                        }
                        break;
                    }
                }

                r.Ret = (int)Result.ResultCode.RC_ok;
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RC_failed;
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
