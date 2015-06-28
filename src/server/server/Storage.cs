using System;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Net;

namespace server
{
    class WeiboInfo
    {
        public string Content { get; set; }
        public string[] Photo { get; set; }
        public uint AccountId { get; set; }
        public string Address { get; set; }
        public string Weather { get; set; }
        public DateTime Time { get; set; }
        public double[] Coordinates { get; set; }
        
        public void FillData(SendWeiboReq info)
        {
            Content = info.Content;
            Photo = info.Photo;
            AccountId = info.AccountId;
            Address = info.Address;
            Weather = info.Weather;
            Time = new DateTime();
            var ts = new TimeSpan(8, 0, 0);
            Time = DateTime.Now + ts;
            Coordinates = new [] { info.Longi, info.Lati };
        }
    }

    class CommentUnit
    {
        public uint AccountId { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public void FillData(CommentWeiboReq info)
        {
            AccountId = info.AccountId;
            Content = info.Content;
            Time = new DateTime();
            var ts = new TimeSpan(8, 0, 0);
            Time = DateTime.Now + ts;
        }
    }

    class CommentInfo
    {
        public ObjectId _id { get; set; }
        public CommentUnit[] ContentArray { get; set; }
    }

    class UserInfo
    {
        public long _id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public long[] Follow { get; set; }
        public long[] Fans { get; set; }
    }

    class LikeInfo
    {
        public ObjectId _id { get; set; }
        public long[] AccountId { get; set; }
    }

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

                var fopt = new FindOptions<BsonDocument> {Limit = 1};

                var f = CollectionMgr.Instance.WeiboBson.FindAsync(filter, fopt);

                using (var cursor = await f)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            r._id = document["_id"].ToString();
                            r.Time = document["Time"].ToUniversalTime();
                            break;
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

        private async void LikeWeibo(HttpListenerResponse rsp, LikeWeiboReq info)
        {
            var r = new Result();

            //将账号id加入到赞db的数组
            var objid = new ObjectId(info._id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objid);
            var update = Builders<BsonDocument>.Update.AddToSet("AccountId", info.AccountId);
            var uop = new UpdateOptions {IsUpsert = true};

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
            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void CommentWeibo(HttpListenerResponse rsp, CommentWeiboReq info)
        {
            var r = new Result();

            try
            {
                var objid = new ObjectId(info._id);
                var unit = new CommentUnit();
                unit.FillData(info);

                var pfilter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var pup = Builders<BsonDocument>.Update.Push("ContentArray", unit);
                var popt = new UpdateOptions {IsUpsert = true};

                var p = CollectionMgr.Instance.CommentBson.UpdateOneAsync(pfilter, pup, popt);
                await p;

                //更新评论数
                var filter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var up = Builders<BsonDocument>.Update.Inc("CommentCount", 1);
                var u = CollectionMgr.Instance.WeiboBson.UpdateOneAsync(filter, up);
                await u;

                r.Ret = (int)Result.ResultCode.RcOk;
            }
            catch
            {
                r.Ret = (int)Result.ResultCode.RcFailed;
            }
            var json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async  void Follow(HttpListenerResponse rsp, FollowReq info)
        {
            var r = new Result();

            var filter = Builders<UserInfo>.Filter.Eq("_id", info.StartId);
            var up = Builders<UserInfo>.Update.AddToSet("follow", info.FollowId);
            var uop = new UpdateOptions {IsUpsert = true};

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

        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.RawUrl)
            {
                case "/sendwb":
                    {
                        var s = GetBody(req);
                        var info = JsonConvert.DeserializeObject<SendWeiboReq>(s);
                        var weiboinfo = new WeiboInfo();
                        weiboinfo.FillData(info);
                        SaveWeibo(req, rsp, weiboinfo);
                    }
                    break;
                case "/likewb":
                    {
                        var s = GetBody(req);
                        var info = JsonConvert.DeserializeObject<LikeWeiboReq>(s);
                        LikeWeibo(rsp, info);
                    }
                    break;
                case "/commentswb":
                    {
                        var s = GetBody(req);
                        var info = JsonConvert.DeserializeObject<CommentWeiboReq>(s);
                        CommentWeibo(rsp, info);
                    }
                    break;
                case "/follow":
                    {
                        var s = GetBody(req);
                        var info = JsonConvert.DeserializeObject<FollowReq>(s);
                        Follow(rsp, info);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
