using System.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;
using System;

namespace server
{


    class Query : HttpSvrBase
    {
        private const int Limit = 3;
        private const int NotLimit = 5;
        private const int Page = 5;
        private const double Raduis = 0.2485484768949336f; //英里 = 1.609344千米(km)，现在是400米半径

        public override void PostHandle(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.Url.AbsolutePath)
            {
                case "/location":
                    {
                        var s = GetBody(req);
                        var info = new LocationQueryReq();
                        if (GetBodyJson<LocationQueryReq>(s, ref info, rsp))
                            QueryLocation(rsp, info);
                    }
                    break;
                case "/locationCenter":
                    {
                        var s = GetBody(req);
                        var info = new LocationQuery2Req();
                        if (GetBodyJson<LocationQuery2Req>(s, ref info, rsp))
                            QueryLocationCenter(rsp, info);
                    }
                    break;
                case "/count":
                    {
                        var s = GetBody(req);
                        var info = new CountQueryReq();
                        if (GetBodyJson<CountQueryReq>(s, ref info, rsp))
                            QueryCount(rsp, info);
                    }
                    break;
                case "/friend":
                    {
                        var s = GetBody(req);
                        var info = new FriendQueryReq();
                        if (GetBodyJson<FriendQueryReq>(s, ref info, rsp))
                            QueryFriend(rsp, info);
                    }
                    break;
                case "/comment":
                    {
                        var s = GetBody(req);
                        var info = new CommentQueryReq();
                        if (GetBodyJson<CommentQueryReq>(s, ref info, rsp))
                            QueryComment(rsp, info);
                    }
                    break;
                case "/like":
                    {
                        var s = GetBody(req);
                        var info = new LikeQueryReq();
                        if (GetBodyJson<LikeQueryReq>(s, ref info, rsp))
                            QueryLike(rsp, info);
                    }
                    break;
                case "/getsign":
                    {
                        var s = GetBody(req);
                        var info = new GetSignReq();
                        if (GetBodyJson<GetSignReq>(s, ref info, rsp))
                            GetSign(rsp, info);
                    }
                    break;
                case "/usericon":
                    {
                        var s = GetBody(req);
                        var info = new GetUserIconReq();
                        if (GetBodyJson<GetUserIconReq>(s, ref info, rsp))
                            QueryUserIcon(rsp, info);
                    }
                    break;
                default:
                    break;
            }

        }

        private async void QueryComment(HttpListenerResponse rsp, CommentQueryReq info)
        {
            var r = new CommentQueryRsp();
            try
            {
                var obj = new ObjectId(info._id);
                var filter = Builders<CommentInfo>.Filter.Eq("_id", obj);
                var proj = Builders<CommentInfo>.Projection.Slice("ContentArray", (info.Skip * NotLimit), NotLimit);
                var p = await CollectionMgr.Instance.CommentInfo.Find(filter).Limit(1).Project<CommentInfo>(proj).ToListAsync();

                foreach(var o in p)
                {
                    foreach(var c in o.ContentArray)
                    {
                        var qu = new CommentQueryUnit();
                        qu.Fill(c);
                        r.Info.Add(qu);
                    }
                    break;
                }
                r.Ret = (int)Result.ResultCode.RcOk;
            }
            catch(Exception e)
            {
                r.ProcException(e);
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void QueryFriend(HttpListenerResponse rsp, FriendQueryReq info)
        {
            var r = new FriendQueryRsp();

            var filter = Builders<UserInfo>.Filter.Eq("_id", info.AccountId);
            var opt = new FindOptions<UserInfo> { Projection = Builders<UserInfo>.Projection.Include("follow") };

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
                                var uopt = new FindOptions<WeiboInfoTotal> { Limit = (info.Preview ? Limit : NotLimit) };
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
                                            var readOpt = new UpdateOptions { IsUpsert = true };
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

                                                var tmp = new WeiboQueryInfo(doc);
                                                r.Info.Add(tmp);
                                            }
                                            else
                                            {
                                                var tmp = new WeiboQueryInfo(doc);
                                                r.Info.Add(tmp);
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
            int limit = (info.Preview ? Limit : NotLimit);
            int skip = (info.Skip * limit);
            var sort = Builders<WeiboInfoTotal>.Sort.Descending("Time");

            try
            {
                var f = await CollectionMgr.Instance.WeiboTotal.Find(filter).Sort(sort).Skip(skip)
                    .Limit(limit).ToListAsync();

                foreach (var doc in f)
                {
                    //浏览次数加1
                    var readFilter = Builders<ReadInfo>.Filter.Eq("_id", doc._id);
                    var readUpate = Builders<ReadInfo>.Update.AddToSet("AccountId", info.AccountId);
                    var readOpt = new UpdateOptions { IsUpsert = true };
                    var re = CollectionMgr.Instance.ReadInfo.UpdateOneAsync(readFilter, readUpate, readOpt);
                    await re;
                    if (re.Result.ModifiedCount == 1)
                    {
                        var readAddFilter = Builders<WeiboInfoTotal>.Filter.Eq("_id", doc._id);
                        var readAddUpdate = Builders<WeiboInfoTotal>.Update.Inc("ReadCount", 1);
                        var readd = CollectionMgr.Instance.WeiboTotal.UpdateOneAsync(readAddFilter, readAddUpdate);
                        await readd;

                        if (readd.Result.ModifiedCount == 1)
                        {
                            doc.ReadCount++;
                        }
                        var tmp = new WeiboQueryInfo(doc);
                        r.Info.Add(tmp);
                    }
                    else
                    {
                        var tmp = new WeiboQueryInfo(doc);
                        r.Info.Add(tmp);
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

        private async void QueryCount(HttpListenerResponse rsp, CountQueryReq info)
        {
            var filter = Builders<WeiboInfoTotal>.Filter.Near("Coordinates", info.Longi, info.Lati)
                & Builders<WeiboInfoTotal>.Filter.Gt("Time", info.LastViewTime);
            var c = CollectionMgr.Instance.WeiboTotal.CountAsync(filter);
            var r = new CountQueryRsp();

            try
            {
                await c;

                r.Ret = (int)Result.ResultCode.RcOk;
                r.Count = c.Result;
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void QueryLike(HttpListenerResponse rsp, LikeQueryReq info)
        {
            var objid = new ObjectId(info._id);
            var filter = Builders<LikeInfo>.Filter.Eq("_id", objid);
            var opt = new FindOptions<LikeInfo>
            {
                Projection = Builders<LikeInfo>.Projection.Slice("AccountId", (info.Skip * NotLimit), NotLimit)
            };

            var l = CollectionMgr.Instance.LikeInfo.FindAsync(filter, opt);

            var r = new LikeQueryRsp();

            try
            {
                using (var cursor = await l)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            r.AccountId = document.AccountId;
                        }
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

        private async void GetSign(HttpListenerResponse rsp, GetSignReq info)
        {
            var r = new GetSignRsp { Ret = (int)Result.ResultCode.RcOk };

            //检查一下该账号是否存在
            var filter = Builders<AccountInfo>.Filter.Eq("AccountId", info.AccountId);
            var c = CollectionMgr.Instance.AccountInfo.CountAsync(filter);
            await c;
            if (c.Result > 0)
            {
                var s = new Sign();
                r.SignStr = s.MakeSign(info.AccountId.ToString());
            }
            else
            {
                r.Ret = (int)Result.ResultCode.RcAccountNotExists;
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void QueryUserIcon(HttpListenerResponse rsp, GetUserIconReq info)
        {
            var r = new GetUserIconRsp();

            var filter = Builders<UserInfo>.Filter.In("_id", info.AccountIdArray);

            try
            {
                var f = await CollectionMgr.Instance.UserInfo.Find(filter).ToListAsync();

                foreach (var document in f)
                {
                    var icon = new UserIcon
                    {
                        AccountId = document._id,
                        Icon = document.Icon,
                        Name = document.Name
                    };

                    r.IconList.Add(icon);
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

        private async void QueryLocationCenter(HttpListenerResponse rsp, LocationQuery2Req info)
        {
            var filter = Builders<WeiboInfoTotal>.Filter.GeoWithinCenter("Coordinates", info.Longi, info.Lati, Raduis);
            var sort = Builders<WeiboInfoTotal>.Sort.Descending("Time");
            var r = new LocationQueryRsp();

            if (info.Time > 0)
            {
                DateTime Time = Timer.TimeStampToDateTime(info.Time);
                filter &= Builders<WeiboInfoTotal>.Filter.Lte("Time", info.Time);
            }

            try
            {
                var f = await CollectionMgr.Instance.WeiboTotal.Find(filter).Sort(sort).Limit(Page).ToListAsync();
                var obj = new ObjectId();
                if (info._id != null && info._id.Length > 0)
                {
                    obj = new ObjectId(info._id);
                }

                foreach (var doc in f)
                {
                    if (info._id != null && info._id.Length > 0)
                    {
                        if (doc._id == obj)
                            continue;
                    }

                    var tmp = new WeiboQueryInfo(doc);
                    r.Info.Add(tmp);
                }

                r.Ret = (int)Result.ResultCode.RcOk;
            }
            catch (Exception e)
            {
                r.ProcException(e);
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }
    }
}
