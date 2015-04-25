using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace server
{
    class LoginReq
    {
        public string act { get; set; }
        public string pwdmdf { get; set; }
    }

    class RegisterReq
    {
        public string act { get; set; }
        public string pwd { get; set; }
        public string pwdmdf { get; set; }
    }

    class SendWeiboReq
    {
        public string content { get; set; }
        public string[] photo { get; set; }
        public double longi { get; set; }
        public double lati { get; set; }
        public long AccountId { get; set; }
        public string address { get; set; }
        public string weather { get; set; }
    }

    class ZanWeiboReq
    {
        public string _id { get; set; }
        public long AccountId { get; set; }
    }

    class PLWeiboReq : ZanWeiboReq
    {
        public string content { get; set; }
    }

    class SendWeiboRsp : Result
    {
        public ObjectId _id { get; set; }
        public DateTime time { get; set; }
    }

    class QueryReq
    {
        public double longi { get; set; }
        public double lati { get; set; }
    }

    class QueryRsp : Result
    {
        public List<WeiboInfoTotal> info = new List<WeiboInfoTotal>();
    }

}
