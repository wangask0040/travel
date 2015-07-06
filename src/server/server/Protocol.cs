using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace server
{
    class LoginReq
    {
        public string Account { get; set; }
        public string Passwdkey { get; set; }
    }

    class LoginRsp : Result
    {
        public uint AccountId { get; set; }
    }

    class RegisterReq
    {
        public string Account { get; set; }
        public string Passwdkey { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

    }

    class PhotoUnit
    {
        public string Path { get; set; }
        public long CreateTime { get; set; }
    }

    class SendWeiboReq
    {
        public string Content { get; set; }
        public PhotoUnit Photo { get; set; }
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
        public long Time { get; set; }
    }

    class LocationQueryReq
    {
        public double Longi { get; set; }
        public double Lati { get; set; }
        public int Skip { get; set; }
        public bool Preview { get; set; }
        public uint AccountId { get; set; }
    }

    class WeiboQueryInfo
    {
        public ObjectId _id { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public string Content { get; set; }
        public string Path { get; set; }
        public uint AccountId { get; set; }
        public string Address { get; set; }
        public string Weather { get; set; }
        public long Time { get; set; }
        public WeiboQueryInfo(WeiboInfoTotal info)
        {
            _id = info._id;
            LikeCount = info.LikeCount;
            CommentCount = info.CommentCount;
            Content = info.Content;
            Path = info.Path;
            AccountId = info.AccountId;
            Address = info.Address;
            Weather = info.Weather;
            Time = Timer.DateTimeToTimeStamp(info.Time);
        }
    }

    class LocationQueryRsp : Result
    {
        public List<WeiboQueryInfo> Info = new List<WeiboQueryInfo>();
    }

    class FollowReq
    {
        public uint StartId { get; set; }
        public uint FollowId { get; set; }
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
        public uint[] AccountId { get; set; }
    }

    class GetSignReq
    {
        public uint AccountId { get; set; }
    }

    class GetSignRsp : Result
    {
        public string SignStr { get; set; }
    }

    class UserIcon
    {
        public uint AccountId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    class GetUserIconReq
    {
        public uint[] AccountIdArray { get; set; }
    }

    class GetUserIconRsp : Result
    {
        public List<UserIcon> IconList = new List<UserIcon>();
    }

    class SetUserIconReq
    {
        public uint AccountId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
