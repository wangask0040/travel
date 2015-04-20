using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Result
    {
        public int result { get; set; }
        public string msg { get; set; }
       
        public enum ResultCode
        {
            RC_ok = 0,
            RC_account_exists = 1,
        }
    }
}
