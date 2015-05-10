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
        private int _result;

        public int Ret { get { return _result; } set { _result = value; Msg = ResultMsg.Instance.Msg(value); } }
        public string Msg { get; private set; }

        public enum ResultCode
        {
            RcOk = 0,
            RcAccountExists = 1,
            RcAccountPasswdNotMatch = 2,
            RcFailed = 3,
            RcAccountNotExists = 4,
            RcAlreayLike = 5,
        }
    }

    class ResultMsg
    {
        public const string ConfigFile = "../../conf/msg.xml";

        public static readonly ResultMsg Instance = new ResultMsg();

        public string Msg(int ret)
        {
            return _mMsg[ret];
        }

        public bool Init(string name)
        {
            _mMsg = new Dictionary<int, string>();
            var doc = new XmlDocument();
            doc.Load(name);
            var root = doc["root"];
            if (root == null) return false;
            foreach (XmlNode item in root.ChildNodes)
            {
                if (item.Attributes == null) continue;
                var id = Convert.ToInt32(item.Attributes["id"].Value);
                _mMsg[id] = item.Attributes["msg"].Value;
            }
            return true;
        }

        private Dictionary<int, string> _mMsg;
    }
}
