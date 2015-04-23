using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace server
{
    class QueryReq
    {
        public double longi { get; set; }
        public double lati { get; set; }
    }

    class WeiboInfoTotal : WeiboInfo
    {
        public ObjectId _id { get; set; }

    }

    class QueryRsp : Result
    {
        public List<WeiboInfoTotal> info = new List<WeiboInfoTotal>();
    }

    class Query : HttpSvrBase
    {
        private IMongoClient m_client;
        private IMongoCollection<WeiboInfoTotal> m_findCollection;

        public Query()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["weibodb"].InnerText);
            m_findCollection = m_client.GetDatabase("db").GetCollection<WeiboInfoTotal>("weibo");
        }

        public override void Proc(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/previewlt":
                    asyQueryJustlt(req, rsp);
                    break;
                default:
                    break;
            }

        }

        private async void asyQueryJustlt(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp)
        {
            QueryRsp r = new QueryRsp();

            QueryReq qr = JsonConvert.DeserializeObject<QueryReq>(GetBody(req));
            var filter = Builders<WeiboInfoTotal>.Filter.Near("geom.coordinates", qr.longi, qr.lati);
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
