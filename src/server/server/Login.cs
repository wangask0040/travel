using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;

namespace server
{
    class Login : HttpSvrBase
    {
        class LoginReq
        {
            public string act { get; set; }
            public string pwdmdf { get; set; }
        }

        private MongoClient m_client;
        private IMongoCollection<AccountInfo> m_collection;

        public Login()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["accountdb"].InnerText);
            m_collection = m_client.GetDatabase("db").GetCollection<AccountInfo>("account");
        }

        public override void Proc(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp)
        {
            string s = GetBody(req);
            asyncFunc(s, rsp);
        }

        private async void asyncFunc(string json, System.Net.HttpListenerResponse rsp)
        {
            LoginReq req = JsonConvert.DeserializeObject<LoginReq>(json);

            AccountInfo info = new AccountInfo();
            info._id = req.act;
            info.Passwd = req.pwdmdf;

            var filter = new BsonDocument();
            filter.Add("_id", info._id);
            filter.Add("Passwd", req.pwdmdf);

            Result r = new Result();
            var f = m_collection.FindAsync(filter);
            await f;

            var cursor = f.Result;
            var c = cursor.MoveNextAsync();
            await c;

            var x = c.Result;
            if (x)
            {
                int count = 0;
                foreach(var doc in cursor.Current)
                {
                    count++;
                }

                if (count == 1)
                {
                    r.result = (int)Result.ResultCode.RC_ok;
                    r.msg = "登录成功！";
                }
                else
                {
                    r.result = (int)Result.ResultCode.RC_account_passwd_not_match;
                    r.msg = "账号密码不匹配！";
                }
            }
            else
            {
                r.result = (int)Result.ResultCode.RC_failed;
                r.msg = "登录失败！";
            }

            string str = JsonConvert.SerializeObject(r);
            byte[] buf = System.Text.Encoding.Default.GetBytes(str);
            rsp.OutputStream.Write(buf, 0, buf.Length);
            rsp.OutputStream.Close();
        }
    }
}
