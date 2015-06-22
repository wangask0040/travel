using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Net;

namespace server
{
    class AccountInfo
    {
        public string _id { get; set; }
        public uint AccountId { get; set; }
        public string Passwd { get; set; }
    }


    class Register : HttpSvrBase
    {
        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            var s = GetBody(req);
            AsyReg(s, rsp);
        }

        private static int Check(RegisterReq info)
        {
            return 0;
        }

        private async void AsyReg(string json, HttpListenerResponse rsp)
        {
            //先将json串，解析成类
            var reginfo = JsonConvert.DeserializeObject<RegisterReq>(json);

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
                    var copt = new CountOptions {Limit = 1};
                    var f = CollectionMgr.Instance.AccountInfo.CountAsync(findfilter, copt);
                    await f;
                    if (f.Result > 0)
                    {
                        r.Ret = (int)Result.ResultCode.RcAccountExists;
                    }
                    else
                    {
                        //先判断有没有
                        var countopt = new CountOptions {Limit = 1};
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

                            //写入db
                            var t = CollectionMgr.Instance.AccountInfo.InsertOneAsync(info);
                            await t;
                        }
                        else
                        {
                            //没有就插入一条
                            var ib = new BsonDocument {{"_id", "AccountCount"}, {"count", 1}};

                            var i = CollectionMgr.Instance.CountBson.InsertOneAsync(ib);
                            await i;

                            //保存账号信息
                            info.AccountId = 1;
                            var ia = CollectionMgr.Instance.AccountInfo.InsertOneAsync(info);
                            await ia;
                        }

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

            //回包
            var str = JsonConvert.SerializeObject(r);
            Response(rsp, str);
        }
    }
}
