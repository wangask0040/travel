using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace server
{
    class Result
    {
        private int result;
        private string msg;

        public int Ret { get { return result; } set { result = value; msg = ResultMsg.Instance.Msg(value); } }
        public string Msg { get { return msg; } private set{} }

        public enum ResultCode
        {
            RC_ok = 0,
            RC_account_exists = 1,
            RC_account_passwd_not_match = 2,
            RC_failed = 3,
            RC_account_not_exists = 4,
        }
    }

    class ResultMsg
    {
        public const string ConfigFile = "../../conf/msg.xml";

        public static readonly ResultMsg Instance = new ResultMsg();

        public string Msg(int ret)
        {
            return m_msg[ret];
        }

        public void Init(string name)
        {
            m_msg = new Dictionary<int, string>();
            XmlDocument doc = new XmlDocument();
            doc.Load(name);
            XmlElement root = doc["root"];
            foreach (XmlNode item in root.ChildNodes)
            {
                int id = Convert.ToInt32(item.Attributes["id"].Value);
                m_msg[id] = item.Attributes["msg"].Value;
            }
        }

        private Dictionary<int, string> m_msg;
    }
}
