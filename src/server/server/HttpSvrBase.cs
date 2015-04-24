﻿using System;
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

        private void Send(HttpListenerContext cxt)
        {

            HttpListenerRequest req = cxt.Request;
            HttpListenerResponse rsp = cxt.Response;

            if (!req.HasEntityBody)
            {
                Console.WriteLine("No client data was sent with the request.");
                return;
            }

            Proc(req, rsp);
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
            System.IO.Stream body = req.InputStream;
            System.Text.Encoding encoding = req.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

            if (req.ContentType != null)
            {
                Console.WriteLine("Client data content type {0}", req.ContentType);
            }

            string s = reader.ReadToEnd();
            Console.WriteLine("recv:{0},lenght:{1}", s, req.ContentLength64);
            body.Close();
            reader.Close();
            return s;
        }

        public void Response(HttpListenerResponse rsp, string str)
        {
            try
            {
                rsp.ContentType = "application/json";
                byte[] buf = System.Text.Encoding.Default.GetBytes(str);
                rsp.OutputStream.Write(buf, 0, buf.Length);
                rsp.OutputStream.Close();
            }
            catch
            {
            }
        }

        private HttpListener m_listen;
    }

}
