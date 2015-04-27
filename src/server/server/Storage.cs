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

    class CommentUnit
    {
        public long AccountId { get; set; }
        public string content { get; set; }
        public DateTime time { get; set; }

        public void FillData(CommentWeiboReq info)
        {
            AccountId = info.AccountId;
            content = info.content;
            time = new DateTime();
            TimeSpan ts = new TimeSpan(8, 0, 0);
            time = DateTime.Now + ts;
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
        public string icon { get; set; }
        public string name { get; set; }
        public long[] follow { get; set; }
        public long[] fans { get; set; }
    }


    class Storage : HttpSvrBase
    {
        public async void SaveWeibo(HttpListenerRequest req, HttpListenerResponse rsp, WeiboInfo info)
        {
            SendWeiboRsp r = new SendWeiboRsp();

            try
            {
                //插入一条
                var t = CollectionMgr.Instance.WeiboInfo.InsertOneAsync(info);
                await t;

                //查找
                var filter = (Builders<BsonDocument>.Filter.Eq("time", info.time)
                    & Builders<BsonDocument>.Filter.Eq("AccountId", info.AccountId));

                var fopt = new FindOptions<BsonDocument>();
                fopt.Limit = 1;

                var f = CollectionMgr.Instance.WeiboBson.FindAsync(filter, fopt);

                using (var cursor = await f)
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            r._id = document["_id"].AsString;
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

        private async void LikeWeibo(HttpListenerRequest req, HttpListenerResponse rsp, LikeWeiboReq info)
        {
            Result r = new Result();

            //将账号id加入到赞db的数组
            ObjectId objid = new ObjectId(info._id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objid);
            var update = Builders<BsonDocument>.Update.AddToSet("AccountId", info.AccountId);
            var uop = new UpdateOptions();
            uop.IsUpsert = true;

            var u = CollectionMgr.Instance.LikeBson.UpdateOneAsync(filter, update, uop);
            await u;

            if (u.Result.ModifiedCount == 1)
            {
                var wfilter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var wupdate = Builders<BsonDocument>.Update.Inc("LikeCount", 1);
                var w = CollectionMgr.Instance.WeiboBson.UpdateOneAsync(wfilter, wupdate);
                await w;
                r.Ret = (int)Result.ResultCode.RC_ok;
            }
            else
            {
                r.Ret = (int)Result.ResultCode.RC_alreay_like;
            }
            string json = JsonConvert.SerializeObject(r);
            Response(rsp, json);
        }

        private async void CommentWeibo(HttpListenerRequest req, HttpListenerResponse rsp, CommentWeiboReq info)
        {
            Result r = new Result();

            try
            {
                ObjectId objid = new ObjectId(info._id);
                CommentUnit unit = new CommentUnit();
                unit.FillData(info);

                var pfilter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var pup = Builders<BsonDocument>.Update.Push("ContentArray", unit);
                var popt = new UpdateOptions();
                popt.IsUpsert = true;

                var p = CollectionMgr.Instance.CommentBson.UpdateOneAsync(pfilter, pup, popt);
                await p;

                //更新评论数
                var filter = Builders<BsonDocument>.Filter.Eq("_id", objid);
                var up = Builders<BsonDocument>.Update.Inc("CommentCount", 1);
                var u = CollectionMgr.Instance.WeiboBson.UpdateOneAsync(filter, up);
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

        private async  void Follow(HttpListenerRequest req, HttpListenerResponse rsp, FollowReq info)
        {
            Result r = new Result();

            var filter = Builders<UserInfo>.Filter.Eq("_id", info.startId);
            var up = Builders<UserInfo>.Update.AddToSet("follow", info.followId);
            var uop = new UpdateOptions();
            uop.IsUpsert = true;

            try
            {
                var t = CollectionMgr.Instance.UserInfo.UpdateOneAsync(filter, up, uop);
                await t;

                filter = Builders<UserInfo>.Filter.Eq("_id", info.followId);
                up = Builders<UserInfo>.Update.AddToSet("follow", info.startId);
                t = CollectionMgr.Instance.UserInfo.UpdateOneAsync(filter, up, uop);
                await t;

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
                case "/likewb":
                    {
                        string s = GetBody(req);
                        LikeWeiboReq info = JsonConvert.DeserializeObject<LikeWeiboReq>(s);
                        LikeWeibo(req, rsp, info);
                    }
                    break;
                case "/commentswb":
                    {
                        string s = GetBody(req);
                        CommentWeiboReq info = JsonConvert.DeserializeObject<CommentWeiboReq>(s);
                        CommentWeibo(req, rsp, info);
                    }
                    break;
                case "/follow":
                    {
                        string s = GetBody(req);
                        FollowReq info = JsonConvert.DeserializeObject<FollowReq>(s);
                        Follow(req, rsp, info);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
