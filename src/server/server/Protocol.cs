using System;
using System.Collections.Generic;

namespace server
{
    class LoginReq
    {
        public string Account { get; set; }
        public string PasswdKey { get; set; }
    }

    class LoginRsp : Result
    {
        public uint AccountId { get; set; }
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
        public uint AccountId { get; set; }
        public string Address { get; set; }
        public string Weather { get; set; }
    }

    class LikeWeiboReq
    {
        public string _id { get; set; }
        public uint AccountId { get; set; }
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
        public uint AccountId { get; set; }
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
        public uint AccountId { get; set; }
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

    class CountQueryReq
    {
        public double Longi { get; set; }
        public double Lati { get; set; }
        public DateTime LastViewTime { get; set; }
    }

    class CountQueryRsp : Result
    {
        public long Count { get; set; }
    }

    class LikeQueryReq
    {
        public string _id { get; set; }
        public int Skip { get; set; }
    }

    class LikeQueryRsp : Result
    {
        public long[] AccountId { get; set; }
    }

    class GetSignReq
    {
        public uint AccountId { get; set; }
    }

    class GetSignRsp : Result
    {
        public string SignStr { get; set; }
    }
}
