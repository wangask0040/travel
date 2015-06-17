using MongoDB.Bson;
using MongoDB.Driver;

namespace server
{
    class CollectionMgr
    {
        public IMongoClient MClient { get; private set; }
        public IMongoCollection<WeiboInfoTotal> WeiboTotal { get; private set; }
        public IMongoCollection<UserInfo> UserInfo { get; private set; }
        public IMongoCollection<CommentInfo> CommentInfo { get; private set; }
        public IMongoCollection<AccountInfo> AccountInfo { get; private set; }
        public IMongoCollection<BsonDocument> CountBson { get; private set; }
        public IMongoCollection<WeiboInfo> WeiboInfo { get; private set; }
        public IMongoCollection<BsonDocument> WeiboBson { get; private set; }
        public IMongoCollection<BsonDocument> LikeBson { get; private set; }
        public IMongoCollection<LikeInfo> LikeInfo { get; private set; }
        public IMongoCollection<BsonDocument> CommentBson { get; private set; }

        public IMongoCollection<ReadInfo> ReadInfo { get; private set; }

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
            LikeInfo = MClient.GetDatabase("db").GetCollection<LikeInfo>("like");

            CommentInfo = MClient.GetDatabase("db").GetCollection<CommentInfo>("comment");
            CommentBson = MClient.GetDatabase("db").GetCollection<BsonDocument>("comment");

            ReadInfo = MClient.GetDatabase("db").GetCollection<ReadInfo>("readinfo");
            return true;
        }
    }
}
