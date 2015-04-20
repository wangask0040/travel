﻿using System;
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
            Register reg = new Register();
            reg.Start(conf.Root["register"].InnerText);

            Login log = new Login();
            log.Start(conf.Root["login"].InnerText);

            Storage stg = new Storage();
            stg.Start(conf.Root["storage"].InnerText);

            Console.ReadLine();
        }
    }
}
