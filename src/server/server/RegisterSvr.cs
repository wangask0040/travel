using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class RegisterSvr : HttpSvrBase
    {
        public override void Proc(System.Net.HttpListenerRequest req, System.Net.HttpListenerResponse rsp)
        {
            Console.WriteLine("wqe");
        }
    }
}
