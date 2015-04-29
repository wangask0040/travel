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
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public long ReadCount { get; set; }
    }

    class ReadInfo
    {
        public ObjectId _id { get; set; }
        public long[] AccountId { get; set; }
    }

    class Query : HttpSvrBase
    {
        private readonly int limit = 3;
        private readonly int notLimit = 5;

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
                case "/comment":
                    {
                        string s = GetBody(req);
                        CommentQueryReq info = JsonConvert.DeserializeObject<CommentQueryReq>(s);
                        QueryComment(req, rsp, info);
                    }
                    break;
                default:
                    break;
            }

        }

        private async void QueryComment(HttpListenerRequest req, HttpListenerResponse rsp, CommentQueryReq info)
        {
            CommentQueryRsp r = new CommentQueryRsp();

            var filter = Builders<CommentInfo>.Filter.Eq("_id", new ObjectId(info._id));
            var opt = new FindOptions<CommentInfo>();
            opt.Projection = Builders<CommentInfo>.Projection.Slice("ContentArray", (info.skip * notLimit), notLimit);
            var f = CollectionMgr.Instance.CommentInfo.FindAsync(filter, opt);

            try
            {
                using (var cursor = await f)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            foreach (var arr in document.ContentArray)
                            {
                                r.info.Add(arr);
                            }

                            r.Ret = (int)Result.ResultCode.RC_ok;
                            break;
                        }
                        break;
                    }
                }
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RC_failed;
            }

            string json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void QueryFriend(HttpListenerRequest req, HttpListenerResponse rsp, FriendQueryReq info)
        {
            FriendQueryRsp r = new FriendQueryRsp();

            var filter = Builders<UserInfo>.Filter.Eq("_id", info.AccountId);
            var opt = new FindOptions<UserInfo>();
            opt.Projection = Builders<UserInfo>.Projection.Include("follow");

            try
            {
                var f = CollectionMgr.Instance.UserInfo.FindAsync(filter, opt);
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

                                var u = CollectionMgr.Instance.WeiboTotal.FindAsync(ufilter, uopt);
                                using (var cs = await u)
                                {
                                    while (await cs.MoveNextAsync())
                                    {
                                        var bat = cs.Current;
                                        foreach (var doc in bat)
                                        {
                                            //浏览次数加1
                                            var readFilter = Builders<ReadInfo>.Filter.Eq("_id", doc._id);
                                            var readUpate = Builders<ReadInfo>.Update.AddToSet("AccountId", info.AccountId);
                                            var readOpt = new UpdateOptions();
                                            readOpt.IsUpsert = true;
                                            var re = CollectionMgr.Instance.ReadInfo.UpdateOneAsync(readFilter, readUpate, readOpt);
                                            await re;
                                            if (re.Result.ModifiedCount == 1)
                                            {
                                                var readAddFilter = Builders<WeiboInfoTotal>.Filter.Eq("_id", document._id);
                                                var readAddUpdate = Builders<WeiboInfoTotal>.Update.Inc("ReadCount", 1);
                                                var readd = CollectionMgr.Instance.WeiboTotal.UpdateOneAsync(readAddFilter, readAddUpdate);
                                                await readd;

                                                if (readd.Result.ModifiedCount == 1)
                                                {
                                                    doc.ReadCount++;
                                                }
                                                r.info.Add(doc);
                                            }
                                            else
                                            {
                                                r.info.Add(doc);
                                            }
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
                var f = CollectionMgr.Instance.WeiboTotal.FindAsync(filter, fopt);

                using (var cursor = await f)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            //浏览次数加1
                            var readFilter = Builders<ReadInfo>.Filter.Eq("_id", document._id);
                            var readUpate = Builders<ReadInfo>.Update.AddToSet("AccountId", info.AccountId);
                            var readOpt = new UpdateOptions();
                            readOpt.IsUpsert = true;
                            var re = CollectionMgr.Instance.ReadInfo.UpdateOneAsync(readFilter, readUpate, readOpt);
                            await re;
                            if (re.Result.ModifiedCount == 1)
                            {
                                var readAddFilter = Builders<WeiboInfoTotal>.Filter.Eq("_id", document._id);
                                var readAddUpdate = Builders<WeiboInfoTotal>.Update.Inc("ReadCount", 1);
                                var readd = CollectionMgr.Instance.WeiboTotal.UpdateOneAsync(readAddFilter, readAddUpdate);
                                await readd;

                                if (readd.Result.ModifiedCount == 1)
                                {
                                    document.ReadCount++;
                                }
                                r.info.Add(document);
                            }
                            else
                            {
                                r.info.Add(document);
                            }

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
