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
    class RegisterReq
    {
        public string act { get; set; }
        public string pwd { get; set; }
        public string pwdmdf { get; set; }
    }

    class AccountInfo
    {
        public string _id { get; set; }
        public long AccountId { get; set; }
        public string Passwd { get; set; }
    }

    class AccountId
    {
        public string _id { get; set; }
        public string count { get; set; }
    }

    class Register : HttpSvrBase
    {
        private MongoClient m_client;
        private IMongoCollection<AccountInfo> m_collection;

        private MongoClient m_accountIdClient;
        private IMongoCollection<BsonDocument> m_accountidCollection;

        public Register()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["accountdb"].InnerText);
            m_collection = m_client.GetDatabase("db").GetCollection<AccountInfo>("account");

            m_accountIdClient = new MongoClient(c.Root["accountid"].InnerText);
            m_accountidCollection = m_accountIdClient.GetDatabase("db").GetCollection<BsonDocument>("count");
        }

        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            string s = GetBody(req);
            asyFunc(s, rsp);
        }

        private int Check(RegisterReq info)
        {
            return 0;
        }

        private async void asyFunc(string json, HttpListenerResponse rsp)
        {
            //先将json串，解析成类
            RegisterReq reginfo = JsonConvert.DeserializeObject<RegisterReq>(json);

            //检查账号名，密码是否合法
            Result r = new Result();
            int ret = Check(reginfo);
            if (ret == 0)
            {
                //账号的数据结构
                AccountInfo info = new AccountInfo();
                info._id = reginfo.act;
                info.Passwd = reginfo.pwdmdf;

                try
                {
                    //                 BsonDocument inc = new BsonDocument();
                    //                 inc.Add("count", 1);
                    //                 BsonDocument doc = new BsonDocument();
                    //                 doc.Add("$inc", inc);
                    //                 UpdateOptions up = new UpdateOptions();
                    //                 up.IsUpsert = true;
                    //                 var u = m_accountidCollection.UpdateOneAsync(filter, doc, up);

                    //先判断账号是否存在
                    var findfilter = Builders<AccountInfo>.Filter.Eq("_id", info._id);
                    CountOptions copt = new CountOptions();
                    copt.Limit = 1;
                    var f = m_collection.CountAsync(findfilter, copt);
                    await f;
                    if (f.Result > 0)
                    {
                        r.Ret = (int)Result.ResultCode.RC_account_exists;
                    }
                    else
                    {
                        //把账号id加1
                        var filter = Builders<BsonDocument>.Filter.Eq("_id", "AccountCount");
                        var updefine = Builders<BsonDocument>.Update.Inc("count", 1);
                        var u = m_accountidCollection.FindOneAndUpdateAsync(filter, updefine);
                        await u;

                        BsonDocument retDoc = (BsonDocument)u.Result;
                        info.AccountId = retDoc["count"].ToInt64();

                        //写入db
                        var t = m_collection.InsertOneAsync(info);
                        await t;

                        r.Ret = (int)Result.ResultCode.RC_ok;
                    }
                }
                catch
                {
                    //发生异常
                    r.Ret = (int)Result.ResultCode.RC_account_exists;
                }
                
            }
            else
            {
                //账号合法性检查
                r.Ret = ret;
            }

            //回包
            string str = JsonConvert.SerializeObject(r);
            Response(rsp, str);
        }
    }
}
