using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using System.Net;

namespace server
{
    class WeiboInfoTotal : WeiboInfo
    {
        public ObjectId _id { get; set; }
        public int ZanCount { get; set; }
        public int PingLunCount { get; set; }
    }

    class Query : HttpSvrBase
    {
        private IMongoClient m_client;
        private IMongoCollection<WeiboInfoTotal> m_findCollection;
        private IMongoCollection<UserInfo> m_userinfoCollection;

        public Query()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["weibodb"].InnerText);
            m_findCollection = m_client.GetDatabase("db").GetCollection<WeiboInfoTotal>("weibo");
            m_userinfoCollection = m_client.GetDatabase("db").GetCollection<UserInfo>("userinfo");
        }

        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/location":
                    asyQueryLocation(req, rsp);
                    break;
                case "/friend":
                    {
                        string s = GetBody(req);
                        FriendQueryReq info = JsonConvert.DeserializeObject<FriendQueryReq>(s);
                        asyQueryFriend(req, rsp, info);
                    }
                    break;
                default:
                    break;
            }

        }

        private async void asyQueryFriend(HttpListenerRequest req, HttpListenerResponse rsp, FriendQueryReq info)
        {
            FriendQueryRsp r = new FriendQueryRsp();

            var filter = Builders<UserInfo>.Filter.Eq("_id", info.AccountId);
            var opt = new FindOptions<UserInfo>();
            opt.Projection = Builders<UserInfo>.Projection.Include("follow");

            try
            {
                var f = m_userinfoCollection.FindAsync(filter, opt);
                using (var cursor = await f)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            if (document.follow.Length > 0)
                            {
                                var ufilter = Builders<WeiboInfoTotal>.Filter.In("AccountId", document.follow.AsEnumerable());
                                var uopt = new FindOptions<WeiboInfoTotal>();
                                uopt.Limit = 3;

                                var u = m_findCollection.FindAsync(ufilter, uopt);
                                using (var cs = await u)
                                {
                                    while (await cs.MoveNextAsync())
                                    {
                                        var bat = cs.Current;
                                        foreach (var doc in bat)
                                        {
                                            r.info.Add(doc);
                                        }
                                        break;
                                    }

                                }
                            }

                            break;
                        }

                        break;
                    }
                    r.Ret = (int)Result.ResultCode.RC_ok;
                }
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RC_failed;
            }

            string json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void asyQueryLocation(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            LocationQueryRsp r = new LocationQueryRsp();

            LocationQueryReq qr = JsonConvert.DeserializeObject<LocationQueryReq>(GetBody(req));
            var filter = Builders<WeiboInfoTotal>.Filter.Near("coordinates", qr.longi, qr.lati);
            var fopt = new FindOptions<WeiboInfoTotal>();
            fopt.Limit = 3;
            fopt.Sort = Builders<WeiboInfoTotal>.Sort.Descending("time");

            try
            {
                var f = m_findCollection.FindAsync(filter, fopt);

                using (var cursor = await f)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            r.info.Add(document);
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
    }
}
