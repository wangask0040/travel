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
        public string Account { get; set; }
        public string PasswdKey { get; set; }
    }

    class LoginRsp : Result
    {
        public long AccountId { get; set; }
    }

    class RegisterReq
    {
        public string Account { get; set; }
        public string Passwdkey { get; set; }
    }

    class SendWeiboReq
    {
        public string Content { get; set; }
        public string[] Photo { get; set; }
        public double Longi { get; set; }
        public double Lati { get; set; }
        public long AccountId { get; set; }
        public string Address { get; set; }
        public string Weather { get; set; }
    }

    class LikeWeiboReq
    {
        public string _id { get; set; }
        public long AccountId { get; set; }
    }

    class CommentWeiboReq : LikeWeiboReq
    {
        public string Content { get; set; }
    }

    class SendWeiboRsp : Result
    {
        public string _id { get; set; }
        public DateTime Time { get; set; }
    }

    class LocationQueryReq
    {
        public double Longi { get; set; }
        public double Lati { get; set; }
        public int Skip { get; set; }
        public bool Preview { get; set; }
        public long AccountId { get; set; }
    }

    class LocationQueryRsp : Result
    {
        public List<WeiboInfoTotal> Info = new List<WeiboInfoTotal>();
    }

    class FollowReq
    {
        public long StartId { get; set; }
        public long FollowId { get; set; }
    }

    class FriendQueryReq
    {
        public long AccountId { get; set; }
        public int Skip { get; set; }
        public bool Preview { get; set; }
    }

    class FriendQueryRsp : LocationQueryRsp
    { }

    class CommentQueryReq
    {
        public string _id { get; set; }
        public int Skip { get; set; }
    }

    class CommentQueryRsp : Result
    {
        public List<CommentUnit> Info = new List<CommentUnit>();
    }
}
