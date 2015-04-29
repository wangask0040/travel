using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace server
{
    class CollectionMgr
    {
        public IMongoClient m_client { get; set; }
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

        public void Init()
        {
            Config c = new Config(Config.ConfigFile);
            m_client = new MongoClient(c.Root["weibodb"].InnerText);

            UserInfo = m_client.GetDatabase("db").GetCollection<UserInfo>("userinfo");

            AccountInfo = m_client.GetDatabase("db").GetCollection<AccountInfo>("account");
            CountBson = m_client.GetDatabase("db").GetCollection<BsonDocument>("count");

            WeiboTotal = m_client.GetDatabase("db").GetCollection<WeiboInfoTotal>("weibo");
            WeiboInfo = m_client.GetDatabase("db").GetCollection<WeiboInfo>("weibo");
            WeiboBson = m_client.GetDatabase("db").GetCollection<BsonDocument>("weibo");

            LikeBson = m_client.GetDatabase("db").GetCollection<BsonDocument>("like");

            CommentInfo = m_client.GetDatabase("db").GetCollection<CommentInfo>("comment");
            CommentBson = m_client.GetDatabase("db").GetCollection<BsonDocument>("comment");

            ReadInfo = m_client.GetDatabase("db").GetCollection<ReadInfo>("readinfo");

        }
    }
}
