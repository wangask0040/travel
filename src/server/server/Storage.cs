using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Storage : HttpSvrBase
    {
        class WeiboInfo
        {

        }

        public override void Proc(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp)
        {
            switch(req.RawUrl)
            {
                case "/sendwb":
                    string s = GetBody(req);

                    break;
                default:
                    break;
            }
        }
    }
}
