using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            Config conf = new Config(Config.ConfigFile);
            RegisterSvr reg = new RegisterSvr();
            reg.Start(conf.Root["register"].InnerText);
            Console.ReadLine();
        }
    }
}
