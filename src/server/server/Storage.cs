using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Net;

namespace server
{
    class WeiboInfo
    {
        public string content { get; set; }
        public string[] photo { get; set; }
        public long AccountId { get; set; }
        public string address { get; set; }
        public string weather { get; set; }
        public DateTime time { get; set; }
        public double[] coordinates { get; set; }
        public void FillData(SendWeiboReq info)
        {
            content = info.content;
            photo = info.photo;
            AccountId = info.AccountId;
            address = info.address;
            weather = info.weather;
            time = new DateTime();
            TimeSpan ts = new TimeSpan(8, 0, 0);
            time = DateTime.Now + ts;
            coordinates = new double[] { info.longi, info.lati };
        }
    }

    class PingLunUnit
    {
        public long AccountId { get; set; }
        public string content { get; set; }
        public DateTime time { get; set; }

        public void FillData(PLWeiboReq info)
        {
            AccountId = info.AccountId;
            content = info.content;
            time = new DateTime();
            TimeSpan ts = new TimeSpan(8, 0, 0);
            time = DateTime.Now + ts;
        }
    }

    class Storage : HttpSvrBase
    {
        private MongoClient m_client;
        private IMongoCollection<WeiboInfo> m_collection;
        private IMongoCollection<BsonDocument> m_findCollection;
        private IMongoCollection<BsonDocument> m_zanCollection;
        private IMongoCollection<BsonDocument> m_pinglunCollection;
        

        public Storage()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["weibodb"].InnerText);
            m_collection = m_client.GetDatabase("db").GetCollection<WeiboInfo>("weibo");
            m_findCollection = m_client.GetDatabase("db").GetCollection<BsonDocument>("weibo");

            m_zanCollection = m_client.GetDatabase("db").GetCollection<BsonDocument>("zandb");
            m_pinglunCollection = m_client.GetDatabase("db").GetCollection<BsonDocument>("pinglundb");
        }

        public async void SaveWeibo(HttpListenerRequest req, HttpListenerResponse rsp, WeiboInfo info)
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

        private async void ZanWeibo(HttpListenerRequest req, HttpListenerResponse rsp, ZanWeiboReq info)
        {
            Result r = new Result();

            //将账号id加入到赞db的数组
            ObjectId objid = new ObjectId(info._id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objid);
            var update = Builders<BsonDocument>.Update.AddToSet("AccountId", info.AccountId);
            var uop = new UpdateOptions();
            uop.IsUpsert = true;

            var u = m_zanCollection.UpdateOneAsync(filter, update, uop);
            await u;

            if (u.Result.ModifiedCount == 1)
            {
                var wfilter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var wupdate = Builders<BsonDocument>.Update.Inc("ZanCount", 1);
                var w = m_findCollection.UpdateOneAsync(wfilter, wupdate);
                await w;
                r.Ret = (int)Result.ResultCode.RC_ok;
            }
            else
            {
                r.Ret = (int)Result.ResultCode.RC_alreay_zan;
            }
            string json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void PingLunWeibo(HttpListenerRequest req, HttpListenerResponse rsp, PLWeiboReq info)
        {
            Result r = new Result();

            try
            {
                ObjectId objid = new ObjectId(info._id);
                PingLunUnit unit = new PingLunUnit();
                unit.FillData(info);

                var pfilter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var pup = Builders<BsonDocument>.Update.Push("ContentArray", unit);
                var popt = new UpdateOptions();
                popt.IsUpsert = true;

                var p = m_pinglunCollection.UpdateOneAsync(pfilter, pup, popt);
                await p;

                //更新评论数
                var filter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var up = Builders<BsonDocument>.Update.Inc("PingLunCount", 1);
                var u = m_findCollection.UpdateOneAsync(filter, up);
                await u;

                r.Ret = (int)Result.ResultCode.RC_ok;
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RC_failed;
            }
            string json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/sendwb":
                    {
                        string s = GetBody(req);
                        SendWeiboReq info = JsonConvert.DeserializeObject<SendWeiboReq>(s);
                        WeiboInfo weiboinfo = new WeiboInfo();
                        weiboinfo.FillData(info);
                        SaveWeibo(req, rsp, weiboinfo);
                    }
                    break;
                case "/zanwb":
                    {
                        string s = GetBody(req);
                        ZanWeiboReq info = JsonConvert.DeserializeObject<ZanWeiboReq>(s);
                        ZanWeibo(req, rsp, info);
                    }
                    break;
                case "/pinglunwb":
                    {
                        string s = GetBody(req);
                        PLWeiboReq info = JsonConvert.DeserializeObject<PLWeiboReq>(s);
                        PingLunWeibo(req, rsp, info);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
