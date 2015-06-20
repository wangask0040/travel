using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace server
{
    class Sign
    {
        public string MakeSign(string userid)
        {
            var str = string.Format(SignStr, AppId, SecretId, GetExpire(), GetCurTimeSec(), GetRand(), userid);
            var h = new HMACSHA1(Encoding.ASCII.GetBytes(SecretKey));
            var b = h.ComputeHash(Encoding.ASCII.GetBytes(str));
            var c = b.Concat(Encoding.ASCII.GetBytes(str)).ToArray();
            return Convert.ToBase64String(c);
        }
        
        private static long GetExpire()
        {
            return ExpireTime + GetCurTimeSec();
        }

        private static long GetCurTimeSec()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        private static int GetRand()
        {
            var r = new Random();
            return r.Next(1, 999999999);
        }

        private const string SignStr = "a={0}&k={1}&e={2}&t={3}&r={4}&u={5}&f=";
        private const int AppId = 201762;
        private const string SecretId = "AKID1CuR1PhRcXisYQTHQEnr18uHDHzsbEgT";
        private const string SecretKey = "vFoH3PPYcryAyTEy0oGqocLfc31IoOhy";
        private const long ExpireTime = (3*24*3600);
    }
}
