using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;

namespace server
{
    class Login : HttpSvrBase
    {
        private MongoClient m_client;
        private IMongoCollection<AccountInfo> m_collection;

        public Login()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["weibodb"].InnerText);
            m_collection = m_client.GetDatabase("db").GetCollection<AccountInfo>("account");
        }

        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            string s = GetBody(req);
            asyncFunc(s, rsp);
        }

        private async void asyncFunc(string json, HttpListenerResponse rsp)
        {
            LoginReq req = JsonConvert.DeserializeObject<LoginReq>(json);

            AccountInfo info = new AccountInfo();
            info._id = req.account;
            info.Passwd = req.passwdkey;

            Result r = new Result();

            //先检查账号是否存在
            var findfilter = Builders<AccountInfo>.Filter.Eq("_id", info._id);
            CountOptions copt = new CountOptions();
            copt.Limit = 1;
            var c = m_collection.CountAsync(findfilter, copt);
            await c;
            if (c.Result == 0)
            {
                r.Ret = (int)Result.ResultCode.RC_account_not_exists;
            }
            else
            {
                findfilter &= Builders<AccountInfo>.Filter.Eq("Passwd", info.Passwd);
                var f = m_collection.CountAsync(findfilter, copt);
                await f;
                if (f.Result == 0)
                {
                    r.Ret = (int)Result.ResultCode.RC_account_passwd_not_match;
                }
                else
                {
                    r.Ret = (int)Result.ResultCode.RC_ok;
                }
            }

            string str = JsonConvert.SerializeObject(r);
            Response(rsp, str);
        }
    }
}
