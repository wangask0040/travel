using System.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
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
        private const int Limit = 3;
        private const int NotLimit = 5;

        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/location":
                    {
                        var s = GetBody(req);
                        var info = JsonConvert.DeserializeObject<LocationQueryReq>(s);
                        QueryLocation(rsp, info);
                    }
                    break;
                case "/friend":
                    {
                        var s = GetBody(req);
                        var info = JsonConvert.DeserializeObject<FriendQueryReq>(s);
                        QueryFriend(rsp, info);
                    }
                    break;
                case "/comment":
                    {
                        var s = GetBody(req);
                        var info = JsonConvert.DeserializeObject<CommentQueryReq>(s);
                        QueryComment(rsp, info);
                    }
                    break;
                default:
                    break;
            }

        }

        private async void QueryComment(HttpListenerResponse rsp, CommentQueryReq info)
        {
            var r = new CommentQueryRsp();

            var filter = Builders<CommentInfo>.Filter.Eq("_id", new ObjectId(info._id));
            var opt = new FindOptions<CommentInfo>
            {
                Projection = Builders<CommentInfo>.Projection.Slice("ContentArray", (info.Skip*NotLimit), NotLimit)
            };
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
                                r.Info.Add(arr);
                            }

                            r.Ret = (int)Result.ResultCode.RcOk;
                            break;
                        }
                        break;
                    }
                }
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void QueryFriend(HttpListenerResponse rsp, FriendQueryReq info)
        {
            var r = new FriendQueryRsp();

            var filter = Builders<UserInfo>.Filter.Eq("_id", info.AccountId);
            var opt = new FindOptions<UserInfo> {Projection = Builders<UserInfo>.Projection.Include("follow")};

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
                            if (document.Follow.Length > 0)
                            {
                                var ufilter = Builders<WeiboInfoTotal>.Filter.In("AccountId", document.Follow.AsEnumerable());
                                var uopt = new FindOptions<WeiboInfoTotal> {Limit = (info.Preview ? Limit : NotLimit)};
                                uopt.Skip = (info.Skip * uopt.Limit);

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
                                            var readOpt = new UpdateOptions {IsUpsert = true};
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
                                                r.Info.Add(doc);
                                            }
                                            else
                                            {
                                                r.Info.Add(doc);
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
                    r.Ret = (int)Result.ResultCode.RcOk;
                }
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void QueryLocation(HttpListenerResponse rsp, LocationQueryReq info)
        {
            var r = new LocationQueryRsp();

            var filter = Builders<WeiboInfoTotal>.Filter.Near("Coordinates", info.Longi, info.Lati);
            var fopt = new FindOptions<WeiboInfoTotal>
            {
                Limit = (info.Preview ? Limit : NotLimit),
                Sort = Builders<WeiboInfoTotal>.Sort.Descending("Time")
            };
            fopt.Skip = (info.Skip * fopt.Limit);

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
                            var readOpt = new UpdateOptions {IsUpsert = true};
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
                                r.Info.Add(document);
                            }
                            else
                            {
                                r.Info.Add(document);
                            }

                        }

                        break;
                    }
                }


                r.Ret = (int)Result.ResultCode.RcOk;
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }
    }
}
