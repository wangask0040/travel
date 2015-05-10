using System.Xml;

namespace server
{
    class Config
    {
        public const string ConfigFile = "../../conf/config.xml";
 
        public XmlElement Root { get { return MRoot; } }

        public Config(string name)
        {
            MDoc = new XmlDocument();
            MDoc.Load(name);
            MRoot = MDoc["root"];
        }

        protected XmlElement MRoot;
        protected XmlDocument MDoc;
    }
}
