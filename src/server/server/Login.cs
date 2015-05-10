using Newtonsoft.Json;
using MongoDB.Driver;
using System.Net;

namespace server
{
    class Login : HttpSvrBase
    {
        public override void Proc(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            var s = GetBody(req);
            AsyncFunc(s, rsp);
        }

        private async void AsyncFunc(string json, HttpListenerResponse rsp)
        {
            var req = JsonConvert.DeserializeObject<LoginReq>(json);

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
                var f = CollectionMgr.Instance.AccountInfo.FindAsync(findfilter);
                using (var cs = await f)
                {

                    if (await cs.MoveNextAsync())
                    {
                        var bat = cs.Current;
                        var it = bat.GetEnumerator();
                        if (it.MoveNext())
                        {
                            r.Ret = (int)Result.ResultCode.RcOk;
                            r.AccountId = it.Current.AccountId;
                        }
                        else
                        {
                            r.Ret = (int)Result.ResultCode.RcAccountPasswdNotMatch;
                        }
                    }
                    else
                    {
                        r.Ret = (int)Result.ResultCode.RcAccountPasswdNotMatch;
                    }
                }
            }

            var str = JsonConvert.SerializeObject(r);
            Response(rsp, str);
        }
    }
}
