using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_Parser
{
    public static class ExpMethods
    {
        private static readonly DateTime _unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly string _vkUrl = "https://vk.com/";

        public static DateTime UnixTimeToDateTime(string timestamp)
        {
            DateTime origin = _unixTime;
            return origin.AddSeconds(double.Parse(timestamp)).ToLocalTime();
        }

        public static string UrlFromID(string id)
        {
            return _vkUrl + "id" + id;
        }

        public static string SexFromNumber(string number)
        {
            return number == "1" ? "female" : "male";
        }
    }
}
