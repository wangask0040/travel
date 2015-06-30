using Newtonsoft.Json;
using MongoDB.Driver;
using System.Net;

namespace server
{
    class Login : HttpSvrBase
    {
        public override void PostHandle(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            var s = GetBody(req);
            var info = JsonConvert.DeserializeObject<LoginReq>(s);
            Lgn(info, rsp);
        }

        public override void GetHandle(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            var info = new LoginReq();
            info.Account = req.QueryString.Get("Account");
            info.PasswdKey = req.QueryString.Get("PasswdKey");
            Lgn(info, rsp);
        }

        private async void Lgn(LoginReq req, HttpListenerResponse rsp)
        {
            var info = new AccountInfo
            {
                _id = req.Account,
                Passwd = req.PasswdKey
            };

            var r = new LoginRsp();

            //先检查账号是否存在
            var findfilter = Builders<AccountInfo>.Filter.Eq("_id", info._id);
            var copt = new CountOptions {Limit = 1};
            var c = CollectionMgr.Instance.AccountInfo.CountAsync(findfilter, copt);
            await c;
            if (c.Result == 0)
            {
                r.Ret = (int)Result.ResultCode.RcAccountNotExists;
            }
            else
            {
                findfilter &= Builders<AccountInfo>.Filter.Eq("Passwd", info.Passwd);
                var f = await CollectionMgr.Instance.AccountInfo.Find(findfilter).ToListAsync();
                if (f.Count == 1)
                {
                    r.Ret = (int)Result.ResultCode.RcOk;
                    r.AccountId = f[0].AccountId;
                }
                else
                {
                    r.Ret = (int)Result.ResultCode.RcAccountPasswdNotMatch;
                }
            }

            var str = JsonConvert.SerializeObject(r);
            Response(rsp, str);
        }
    }
}
