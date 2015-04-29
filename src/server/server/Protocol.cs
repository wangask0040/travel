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
        public string account { get; set; }
        public string passwdkey { get; set; }
    }

    class LoginRsp : Result
    {
        public long AccountId { get; set; }
    }

    class RegisterReq
    {
        public string account { get; set; }
        public string passwdkey { get; set; }
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

    class LikeWeiboReq
    {
        public string _id { get; set; }
        public long AccountId { get; set; }
    }

    class CommentWeiboReq : LikeWeiboReq
    {
        public string content { get; set; }
    }

    class SendWeiboRsp : Result
    {
        public string _id { get; set; }
        public DateTime time { get; set; }
    }

    class LocationQueryReq
    {
        public double longi { get; set; }
        public double lati { get; set; }
        public int skip { get; set; }
        public bool preview { get; set; }
        public long AccountId { get; set; }
    }

    class LocationQueryRsp : Result
    {
        public List<WeiboInfoTotal> info = new List<WeiboInfoTotal>();
    }

    class FollowReq
    {
        public long startId { get; set; }
        public long followId { get; set; }
    }

    class FriendQueryReq
    {
        public long AccountId { get; set; }
        public int skip { get; set; }
        public bool preview { get; set; }
    }

    class FriendQueryRsp : LocationQueryRsp
    { }

    class CommentQueryReq
    {
        public string _id { get; set; }
        public int skip { get; set; }
    }

    class CommentQueryRsp : Result
    {
        public List<CommentUnit> info = new List<CommentUnit>();
    }
}
