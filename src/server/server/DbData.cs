
using MongoDB.Bson;
using System;

namespace server
{
    //用户账号信息
    class AccountInfo
    {
        public string _id { get; set; }
        public long AccountId { get; set; }
        public string Passwd { get; set; }
    }

    //完整微博信息
    class WeiboInfoTotal : WeiboInfo
    {
        public ObjectId _id { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public long ReadCount { get; set; }
    }

    //微博被浏览的信息（这个功能后面砍掉了）
    class ReadInfo
    {
        public ObjectId _id { get; set; }
        public long[] AccountId { get; set; }
    }

    //微博部分信息，还有一部分是由后台生成的
    class WeiboInfo
    {
        public string Content { get; set; }
        public string[] Photo { get; set; }
        public long AccountId { get; set; }
        public string Address { get; set; }
        public string Weather { get; set; }
        public DateTime Time { get; set; }
        public double[] Coordinates { get; set; }

        public void FillData(SendWeiboReq info)
        {
            Content = info.Content;
            Photo = info.Photo;
            AccountId = info.AccountId;
            Address = info.Address;
            Weather = info.Weather;
            Time = new DateTime();
            var ts = new TimeSpan(8, 0, 0);
            Time = DateTime.Now + ts;
            Coordinates = new[] { info.Longi, info.Lati };
        }
    }

    //评论的基础结构
    class CommentUnit
    {
        public long AccountId { get; set; }
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

    //评论信息
    class CommentInfo
    {
        public ObjectId _id { get; set; }
        public CommentUnit[] ContentArray { get; set; }
    }

    //用户的个人信息
    class UserInfo
    {
        public long _id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public long[] Follow { get; set; }
        public long[] Fans { get; set; }
    }

    //点赞信息
    class LikeInfo
    {
        public ObjectId _id { get; set; }
        public long[] AccountId { get; set; }
    }
}
