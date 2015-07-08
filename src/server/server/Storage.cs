using System;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Net;

namespace server
{
    class Storage : HttpSvrBase
    {
        public async void SaveWeibo(HttpListenerRequest req, HttpListenerResponse rsp, WeiboInfo info)
        {
            var r = new SendWeiboRsp();

            try
            {
                //插入一条
                var t = CollectionMgr.Instance.WeiboInfo.InsertOneAsync(info);
                await t;

                //查找
                var filter = (Builders<BsonDocument>.Filter.Eq("Time", info.Time)
                    & Builders<BsonDocument>.Filter.Eq("AccountId", info.AccountId));

                var f = await CollectionMgr.Instance.WeiboBson.Find(filter).Limit(1).ToListAsync();
                if (f.Count == 1)
                {
                    r._id = f[0]["_id"].ToString();
                    r.Time = Timer.DateTimeToTimeStamp(f[0]["Time"].ToLocalTime());
                    r.Ret = (int)Result.ResultCode.RcOk;
                }
                else
                {
                    r.Ret = (int)Result.ResultCode.RcFailed;
                }
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void LikeWeibo(HttpListenerResponse rsp, LikeWeiboReq info)
        {
            var r = new Result();

            //将账号id加入到赞db的数组
            try
            {
                var objid = new ObjectId(info._id);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var update = Builders<BsonDocument>.Update.AddToSet("AccountId", info.AccountId);
                var uop = new UpdateOptions { IsUpsert = true };

                var u = CollectionMgr.Instance.LikeBson.UpdateOneAsync(filter, update, uop);
                await u;

                if (u.Result.ModifiedCount == 1)
                {
                    var wfilter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                    var wupdate = Builders<BsonDocument>.Update.Inc("LikeCount", 1);
                    var w = CollectionMgr.Instance.WeiboBson.UpdateOneAsync(wfilter, wupdate);
                    await w;
                    r.Ret = (int)Result.ResultCode.RcOk;
                }
                else
                {
                    r.Ret = (int)Result.ResultCode.RcAlreayLike;
                }
            }
            catch(Exception e)
            {
                r.ProcException(e);
            }
            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void CommentWeibo(HttpListenerResponse rsp, CommentWeiboReq info)
        {
            var r = new Result();

            try
            {
                var obj = new ObjectId(info._id);
                var unit = new CommentUnit();
                unit.FillData(info);

                var pfilter = Builders<BsonDocument>.Filter.Eq("_id", obj);
                var pup = Builders<BsonDocument>.Update.Push("ContentArray", unit);
                var popt = new UpdateOptions { IsUpsert = true };

                var p = CollectionMgr.Instance.CommentBson.UpdateOneAsync(pfilter, pup, popt);
                await p;

                //更新评论数
                var filter = Builders<BsonDocument>.Filter.Eq("_id", obj);
                var up = Builders<BsonDocument>.Update.Inc("CommentCount", 1);
                var u = CollectionMgr.Instance.WeiboBson.UpdateOneAsync(filter, up);
                await u;

                r.Ret = (int)Result.ResultCode.RcOk;
            }
            catch(Exception e)
            {
                r.ProcException(e);
            }
            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void Follow(HttpListenerResponse rsp, FollowReq info)
        {
            var r = new Result();

            var filter = Builders<UserInfo>.Filter.Eq("_id", info.StartId);
            var up = Builders<UserInfo>.Update.AddToSet("follow", info.FollowId);
            var uop = new UpdateOptions { IsUpsert = true };

            try
            {
                var t = CollectionMgr.Instance.UserInfo.UpdateOneAsync(filter, up, uop);
                await t;

                filter = Builders<UserInfo>.Filter.Eq("_id", info.FollowId);
                up = Builders<UserInfo>.Update.AddToSet("follow", info.StartId);
                t = CollectionMgr.Instance.UserInfo.UpdateOneAsync(filter, up, uop);
                await t;

                r.Ret = (int)Result.ResultCode.RcOk;
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }
            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void SetIcon(HttpListenerResponse rsp, SetUserIconReq info)
        {
            var r = new Result();
            var fliter = Builders<UserInfo>.Filter.Eq("_id", info.AccountId);
            var update = Builders<UserInfo>.Update.Set("Icon", info.Icon);

            var uop = new UpdateOptions { IsUpsert = true };

            try
            {
                var u = CollectionMgr.Instance.UserInfo.UpdateOneAsync(fliter, update, uop);
                await u;

                var update2 = Builders<UserInfo>.Update.Set("Name", info.Name);
                var u2 = CollectionMgr.Instance.UserInfo.UpdateOneAsync(fliter, update2);
                await u2;

                if (u.Result.ModifiedCount == 1 || u2.Result.ModifiedCount == 1)
                {
                    r.Ret = (int)Result.ResultCode.RcOk;
                }
                else
                {
                    r.Ret = (int)Result.ResultCode.RcFailed;
                }
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }

            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        public override void PostHandle(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.Url.AbsolutePath)
            {
                case "/sendwb":
                    {
                        var s = GetBody(req);
                        var info = new SendWeiboReq();
                        if (GetBodyJson<SendWeiboReq>(s, ref info, rsp))
                        {
                            var weiboinfo = new WeiboInfo();
                            weiboinfo.FillData(info);
                            SaveWeibo(req, rsp, weiboinfo);
                        }
                    }
                    break;
                case "/likewb":
                    {
                        var s = GetBody(req);
                        var info = new LikeWeiboReq();
                        if (GetBodyJson<LikeWeiboReq>(s, ref info, rsp))
                            LikeWeibo(rsp, info);
                    }
                    break;
                case "/commentswb":
                    {
                        var s = GetBody(req);
                        var info = new CommentWeiboReq();
                        if (GetBodyJson<CommentWeiboReq>(s, ref info, rsp))
                            CommentWeibo(rsp, info);
                    }
                    break;
                case "/follow":
                    {
                        var s = GetBody(req);
                        var info = new FollowReq();
                        if (GetBodyJson<FollowReq>(s, ref info, rsp))
                            Follow(rsp, info);
                    }
                    break;
                case "/seticon":
                    {
                        var s = GetBody(req);
                        var info = new SetUserIconReq();
                        if (GetBodyJson<SetUserIconReq>(s, ref info, rsp))
                            SetIcon(rsp, info);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
