using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MongoDB.Bson;
using MongoDB.Driver;

namespace server
{
    class CollectionMgr
    {
        public IMongoClient MClient { get; set; }
        public IMongoCollection<WeiboInfoTotal> WeiboTotal { get; set; }
        public IMongoCollection<UserInfo> UserInfo { get; set; }
        public IMongoCollection<CommentInfo> CommentInfo { get; set; }
        public IMongoCollection<AccountInfo> AccountInfo { get; set; }
        public IMongoCollection<BsonDocument> CountBson { get; set; }
        public IMongoCollection<WeiboInfo> WeiboInfo { get; set; }
        public IMongoCollection<BsonDocument> WeiboBson { get; set; }
        public IMongoCollection<BsonDocument> LikeBson { get; set; }
        public IMongoCollection<BsonDocument> CommentBson { get; set; }

        public IMongoCollection<ReadInfo> ReadInfo { get; set; }

        public static readonly CollectionMgr Instance = new CollectionMgr();

        public bool Init()
        {
            var c = new Config(Config.ConfigFile);
            var xmlElement = c.Root["weibodb"];
            if (xmlElement != null)
                MClient = new MongoClient(xmlElement.InnerText);
            else
                return false;

            UserInfo = MClient.GetDatabase("db").GetCollection<UserInfo>("userinfo");

            AccountInfo = MClient.GetDatabase("db").GetCollection<AccountInfo>("account");
            CountBson = MClient.GetDatabase("db").GetCollection<BsonDocument>("count");

            WeiboTotal = MClient.GetDatabase("db").GetCollection<WeiboInfoTotal>("weibo");
            WeiboInfo = MClient.GetDatabase("db").GetCollection<WeiboInfo>("weibo");
            WeiboBson = MClient.GetDatabase("db").GetCollection<BsonDocument>("weibo");

            LikeBson = MClient.GetDatabase("db").GetCollection<BsonDocument>("like");

            CommentInfo = MClient.GetDatabase("db").GetCollection<CommentInfo>("comment");
            CommentBson = MClient.GetDatabase("db").GetCollection<BsonDocument>("comment");

            ReadInfo = MClient.GetDatabase("db").GetCollection<ReadInfo>("readinfo");
            return true;
        }
    }
}
