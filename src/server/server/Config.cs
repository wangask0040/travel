using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace server
{
    class Config
    {
        public const string ConfigFile = "../../conf/config.xml";
 
        public XmlElement Root { get { return m_root; } }

        public Config(string name)
        {
            m_doc = new XmlDocument();
            m_doc.Load(name);
            m_root = m_doc["root"];
        }

        private XmlElement m_root;
        private XmlDocument m_doc;
    }
}
