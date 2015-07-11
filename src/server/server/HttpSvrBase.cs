using System;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace server
{
    class HttpSvrBase
    {
        public void Start(string url)
        {
            _listen = new HttpListener();
            _listen.Prefixes.Add(url);
            _listen.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listen.Start();

            Console.WriteLine("start listening {0}", url);

            Get(_listen);
        }

        public virtual void PostHandle(HttpListenerRequest req, HttpListenerResponse rsp) { }
        public virtual void GetHandle(HttpListenerRequest req, HttpListenerResponse rsp) { }

        private void Send(HttpListenerContext cxt)
        {

            var req = cxt.Request;
            var rsp = cxt.Response;

            //             if (!req.HasEntityBody)
            //             {
            //                 Console.WriteLine("No client data was sent with the request.");
            //                 return;
            //             }

            RouteMethod(req, rsp);
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

        public string GetBody(HttpListenerRequest req)
        {
            var body = req.InputStream;
            var encoding = req.ContentEncoding;
            var reader = new System.IO.StreamReader(body, encoding);

            Console.WriteLine("content type:{0},encoding:{1},lenght:{2},time:{3}",
                    req.ContentType != null ? req.ContentType : "null", encoding.BodyName, 
                    req.ContentLength64, DateTime.Now);

            var s = reader.ReadToEnd();
            Console.WriteLine("recv:{0}", s);

            body.Close();
            reader.Close();
            return s;
        }

        public void Response(HttpListenerResponse rsp, Result r)
        {
            try
            {
                string str = JsonConvert.SerializeObject(r);
                rsp.ContentType = "application/json;charset=gb2312";
                var buf = Encoding.Default.GetBytes(str);
                rsp.OutputStream.Write(buf, 0, buf.Length);
                rsp.OutputStream.Close();
            }
            catch
            {
                // ignored
            }
        }

        protected void RouteMethod(HttpListenerRequest req, HttpListenerResponse rsp)
        {
            switch (req.HttpMethod)
            {
                case "POST":
                    if (req.HasEntityBody)
                        PostHandle(req, rsp);
                    break;
                case "GET":
                    Console.WriteLine("recv:{0},time:{1}", req.Url, DateTime.Now);
                    GetHandle(req, rsp);
                    break;
            }
        }

        protected bool GetBodyJson<T>(string s, ref T t, HttpListenerResponse rsp)
        {
            try
            {
                t = JsonConvert.DeserializeObject<T>(s);
                return true;
            }
            catch (Exception e)
            {
                var r = new Result();
                r.ProcException(e);
                Response(rsp, r);
                return false;
            }
        }

        private HttpListener _listen;
    }

}
