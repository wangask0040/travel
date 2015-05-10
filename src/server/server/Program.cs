using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            ResultMsg.Instance.Init(ResultMsg.ConfigFile);

            CollectionMgr.Instance.Init();

            var conf = new Config(Config.ConfigFile);
            var reg = new Register();
            var xmlElement = conf.Root["register"];
            if (xmlElement != null) 
                reg.Start(xmlElement.InnerText);
            else
                return;

            var log = new Login();
            var element = conf.Root["login"];
            if (element != null) 
                log.Start(element.InnerText);
            else
                return;

            var stg = new Storage();
            var xmlElement1 = conf.Root["storage"];
            if (xmlElement1 != null) 
                stg.Start(xmlElement1.InnerText);
            else 
                return;

            var qry = new Query();
            var element1 = conf.Root["query"];
            if (element1 != null)
                qry.Start(element1.InnerText);
            else 
                return;

            Console.ReadLine();
        }
    }
}
