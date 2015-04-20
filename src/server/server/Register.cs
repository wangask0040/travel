using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;

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
        public string Passwd { get; set; }
    }

    class Register : HttpSvrBase
    {
        private MongoClient m_client;
        private IMongoCollection<AccountInfo> m_collection;

        public Register()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["accountdb"].InnerText);
            m_collection = m_client.GetDatabase("db").GetCollection<AccountInfo>("account");
        }

        public override void Proc(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp)
        {
            string s = GetBody(req);
            asyFunc(s, rsp);
        }

        private int Check(RegisterReq info)
        {
            return 0;
        }

        private async void asyFunc(string json, System.Net.HttpListenerResponse rsp)
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
                    //写入db
                    var t = m_collection.InsertOneAsync(info);
                    await t;
                    r.result = (int)Result.ResultCode.RC_ok;
                    r.msg = "注册成功！";
                }
                catch
                {
                    //发生异常
                    r.result = (int)Result.ResultCode.RC_account_exists;
                    r.msg = "账号已存在！";
                }
                
            }
            else
            {
                //账号合法性检查
                r.result = ret;
                r.msg = "注册失败！";
            }

            //回包
            string str = JsonConvert.SerializeObject(r);
            Response(rsp, str);
        }
    }
}
