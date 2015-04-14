using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace server
{
    class HttpSvrBase
    {
        public void Start(string url)
        {
            m_listen = new HttpListener();
            m_listen.Prefixes.Add(url);
            m_listen.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            m_listen.Start();

            Console.WriteLine("start listening {0}", url);

            Get(m_listen);
        }

        public virtual void Proc(HttpListenerRequest req, HttpListenerResponse rsp) { }

        private async void Send(HttpListenerContext cxt)
        {
            var t = Task.Factory.StartNew(() =>
                {
                    HttpListenerRequest req = cxt.Request;
                    HttpListenerResponse rsp = cxt.Response;
                    Proc(req, rsp);
                });
            await t;
        }

        public async void Get(HttpListener lst)
        {
            while (true)
            {
                var t = lst.GetContextAsync();
                await t;

                Send(t.Result);
            }
        }

        private HttpListener m_listen;
    }

}
