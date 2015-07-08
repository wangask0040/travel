using System;
using System.Collections.Generic;
using System.Xml;

namespace server
{
    class Result
    {
        private int _result;

        public int Ret { get { return _result; } set { _result = value; Msg = ResultMsg.Instance.Msg(value); } }
        public string Msg { get; private set; }
        public void ProcException(Exception e)
        {
            var tmp = e as ArgumentOutOfRangeException;
            if (tmp != null)
            {
                Ret = (int)ResultCode.RcObjecidStrErr;
            }
            else
            {
                Ret = (int)ResultCode.RcFailed;
            }
        }

        public enum ResultCode
        {
            RcOk = 0,
            RcAccountExists = 1,
            RcAccountPasswdNotMatch = 2,
            RcFailed = 3,
            RcAccountNotExists = 4,
            RcAlreayLike = 5,
            RcJsonFormatErr = 6,
            RcObjecidStrErr = 7,
        }
    }

    class ResultMsg
    {
        public const string ConfigFile = "../../conf/msg.xml";

        public static readonly ResultMsg Instance = new ResultMsg();

        public string Msg(int ret)
        {
            return _msg[ret];
        }

        public bool Init(string name)
        {
            _msg = new Dictionary<int, string>();
            var doc = new XmlDocument();
            doc.Load(name);
            var root = doc["root"];
            if (root == null) return false;
            foreach (XmlNode item in root.ChildNodes)
            {
                if (item.Attributes == null) continue;
                var id = Convert.ToInt32(item.Attributes["id"].Value);
                _msg[id] = item.Attributes["msg"].Value;
            }
            return true;
        }

        private Dictionary<int, string> _msg;
    }
}
