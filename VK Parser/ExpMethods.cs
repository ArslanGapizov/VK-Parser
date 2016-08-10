using System;
using System.Collections.Generic;
using System.IO;
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
        public static async Task<bool> WriteToCSV(IEnumerable<User> usersData, string filePath)
        {
            var csv = new StringBuilder();


            var header = "id,FirstName,LastName,Sex,BDate,Country,City,PM,MobilePhone,HomePhone,Time,Relation,Partner";
            csv.AppendLine(header);

            try
            {
                object lockMe = new object();
                Parallel.ForEach(usersData, user =>
                {
                    if (user != null)
                    {
                        lock (lockMe)
                        {
                            object[] row =
                            {
                            string.Format("\"{0}\"", user.Id),
                            string.Format("\"{0}\"", user.FirstName),
                            string.Format("\"{0}\"", user.LastName),
                            string.Format("\"{0}\"", user.Sex),
                            string.Format("\"{0}\"", user.BDate),
                            string.Format("\"{0}\"", user.Country),
                            string.Format("\"{0}\"", user.City),
                            string.Format("\"{0}\"", user.PrivateMessage),
                            string.Format("\"{0}\"", user.MobilePhone),
                            string.Format("\"{0}\"", user.HomePhone),
                            string.Format("\"{0}\"", user.Time.ToString("yyyy-MM-dd HH:mm:ss")),
                            string.Format("\"{0}\"", user.Relation),
                            string.Format("\"{0}\"", user.Partner)
                            };
                            csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", row));
                        }
                    }
                });

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }


    }
}
