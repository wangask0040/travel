using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Timer
    {
        //取当前的时间戳
        public static long GetCurTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        //将DateTime转换成时间戳
        public static long DateTimeToTimeStamp(DateTime dt)
        {
            DateTime d = new DateTime();
            d = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan s = (dt - d);
            return Convert.ToInt64(s.TotalSeconds);
        }

        //将时间戳转换成DateTime
        public static DateTime TimeStampToDateTime(long sec)
        {
            DateTime d = new DateTime();
            d = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var s = new TimeSpan(sec * 10000000);
            d += s;
            return d;
        }
    }
}
