using System;
using MongoDB.Bson;

namespace server
{
    class AccountInfo
    {
        public string _id { get; set; }
        public uint AccountId { get; set; }
        public string Passwd { get; set; }
    }
    
    class PhotoInfo
    {
        public string Path { get; set; }
        public DateTime CreateTime { get; set; }
    }

    class WeiboInfo
    {
        public string Content { get; set; }
        public PhotoInfo Photo { get; set; }
        public uint AccountId { get; set; }
        public string Address { get; set; }
        public string Weather { get; set; }
        public DateTime Time { get; set; }
        public double[] Coordinates { get; set; }

        public void FillData(SendWeiboReq info)
        {
            Content = info.Content;
            Photo = new PhotoInfo();

            Photo.Path = info.Photo.Path;
            Photo.CreateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var s = new TimeSpan(info.Photo.CreateTime * 10000000);
            Photo.CreateTime += s;

            AccountId = info.AccountId;
            Address = info.Address;
            Weather = info.Weather;

            Time = new DateTime();
            var ts = new TimeSpan(8, 0, 0);
            Time = DateTime.Now + ts;

            Coordinates = new[] { info.Longi, info.Lati };
        }
    }

    class CommentUnit
    {
        public uint AccountId { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public void FillData(CommentWeiboReq info)
        {
            AccountId = info.AccountId;
            Content = info.Content;
            Time = new DateTime();
            var ts = new TimeSpan(8, 0, 0);
            Time = DateTime.Now + ts;
        }
    }

    class CommentInfo
    {
        public ObjectId _id { get; set; }
        public CommentUnit[] ContentArray { get; set; }
    }

    class UserInfo
    {
        public uint _id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public uint[] Follow { get; set; }
        public uint[] Fans { get; set; }
    }

    class LikeInfo
    {
        public ObjectId _id { get; set; }
        public uint[] AccountId { get; set; }
    }

    class WeiboInfoTotal : WeiboInfo
    {
        public ObjectId _id { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public uint ReadCount { get; set; }
    }

    class ReadInfo
    {
        public ObjectId _id { get; set; }
        public uint[] AccountId { get; set; }
    }

}
