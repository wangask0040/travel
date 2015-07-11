using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Net;

namespace server
{
    class Register : HttpSvrBase
    {
        public override void PostHandle(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            var s = GetBody(req);
            var info = new RegisterReq();
            if (GetBodyJson<RegisterReq>(s, ref info, rsp))
                Reg(info, rsp);
        }

        public override void GetHandle(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            var info = new RegisterReq();
            info.Account = req.QueryString.Get("Account");
            info.Passwdkey = req.QueryString.Get("Passwdkey");
            Reg(info, rsp);
        }

        private static int Check(RegisterReq info)
        {
            return 0;
        }

        private async void Reg(RegisterReq reginfo, HttpListenerResponse rsp)
        {
            //检查账号名，密码是否合法
            var r = new Result();
            var ret = Check(reginfo);
            if (ret == 0)
            {
                //账号的数据结构
                var info = new AccountInfo
                {
                    _id = reginfo.Account,
                    Passwd = reginfo.Passwdkey
                };

                try
                {
                    //先判断账号是否存在
                    var findfilter = Builders<AccountInfo>.Filter.Eq("_id", info._id);
                    var copt = new CountOptions { Limit = 1 };
                    var f = CollectionMgr.Instance.AccountInfo.CountAsync(findfilter, copt);
                    await f;
                    if (f.Result > 0)
                    {
                        r.Ret = (int)Result.ResultCode.RcAccountExists;
                    }
                    else
                    {
                        //先判断有没有
                        var countopt = new CountOptions { Limit = 1 };
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", "AccountCount");
                        var c = CollectionMgr.Instance.CountBson.CountAsync(filter, countopt);
                        await c;

                        if (c.Result > 0)
                        {
                            //把账号id加1
                            var updefine = Builders<BsonDocument>.Update.Inc("count", 1);
                            var u = CollectionMgr.Instance.CountBson.FindOneAndUpdateAsync(filter, updefine);
                            await u;

                            var retDoc = u.Result;
                            info.AccountId = (uint)(retDoc["count"].ToInt32() + 1);

                        }
                        else
                        {
                            //没有就插入一条
                            var ib = new BsonDocument { { "_id", "AccountCount" }, { "count", 1 } };

                            var i = CollectionMgr.Instance.CountBson.InsertOneAsync(ib);
                            await i;

                            info.AccountId = 1;
                        }

                        //写入db
                        var t = CollectionMgr.Instance.AccountInfo.InsertOneAsync(info);
                        await t;

                        //用户的昵称和Icon写入
                        var icon = new UserInfo
                        {
                            _id = info.AccountId,
                            Name = reginfo.Name,
                            Icon = reginfo.Icon
                        };

                        await CollectionMgr.Instance.UserInfo.InsertOneAsync(icon);

                        r.Ret = (int)Result.ResultCode.RcOk;
                    }
                }
                catch
                {
                    //发生异常
                    r.Ret = (int)Result.ResultCode.RcAccountExists;
                }
            }
            else
            {
                //账号合法性检查
                r.Ret = ret;
            }

            Response(rsp, r);
        }
    }
}
