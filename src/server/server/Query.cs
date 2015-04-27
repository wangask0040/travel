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
        private readonly int limit = 3;
        private readonly int notLimit = 5;

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
                    {
                        string s = GetBody(req);
                        LocationQueryReq info = JsonConvert.DeserializeObject<LocationQueryReq>(s);
                        QueryLocation(req, rsp, info);
                    }
                    break;
                case "/friend":
                    {
                        string s = GetBody(req);
                        FriendQueryReq info = JsonConvert.DeserializeObject<FriendQueryReq>(s);
                        QueryFriend(req, rsp, info);
                    }
                    break;
                default:
                    break;
            }

        }

        private async void QueryFriend(HttpListenerRequest req, HttpListenerResponse rsp, FriendQueryReq info)
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
                                uopt.Limit = (info.preview ? limit : notLimit);
                                uopt.Skip = (info.skip * uopt.Limit);

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

        private async void QueryLocation(HttpListenerRequest req, HttpListenerResponse rsp, LocationQueryReq info)
        {
            LocationQueryRsp r = new LocationQueryRsp();

            var filter = Builders<WeiboInfoTotal>.Filter.Near("coordinates", info.longi, info.lati);
            var fopt = new FindOptions<WeiboInfoTotal>();
            fopt.Limit = (info.preview ? limit : notLimit);
            fopt.Sort = Builders<WeiboInfoTotal>.Sort.Descending("time");
            fopt.Skip = (info.skip * fopt.Limit);

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
