using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace server
{
    class REGISTER_INFO
    {
        public string act { get; private set; }
        public string pwd { get; private set; }
        public string pwdmdf { get; private set; }
    }

    class RegisterSvr : HttpSvrBase
    {
        private MongoClient m_client;

        public RegisterSvr()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["accountdb"].InnerText);
        }

        public override void Proc(string recv, System.Net.HttpListenerResponse rsp)
        {
            Save(recv);

            byte[] buf = System.Text.Encoding.Default.GetBytes(recv);
            rsp.OutputStream.Write(buf, 0, buf.Length);
            rsp.OutputStream.Close();
        }

        private int Check(REGISTER_INFO info)
        {
            return 0;
        }

        private int Save(string json)
        {
            REGISTER_INFO reginfo = (REGISTER_INFO)JsonConvert.DeserializeObject(json, typeof(REGISTER_INFO));
            int ret = Check(reginfo);
            if (ret != 0)
                return ret;


            return 0;
        }
    }
}
