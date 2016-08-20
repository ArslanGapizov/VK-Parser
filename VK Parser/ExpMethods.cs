using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;

namespace VK_Parser
{
    public static class ExpMethods
    {
        /*unix data time equal to zero*/
        private static readonly DateTime _unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly string _vkUrl = "https://vk.com/";
        /*method for converting unix timestamp to DateTime*/
        public static DateTime UnixTimeToDateTime(string timestamp)
        {
            DateTime origin = _unixTime;
            return origin.AddSeconds(double.Parse(timestamp)).ToLocalTime();
        }
        /*transfrom id to url*/
        public static string UrlFromID(string id)
        {
            return _vkUrl + "id" + id;
        }
        /*returns sex according to it`s id*/
        public static string SexFromNumber(string number)
        {
            return number == "1" ? "female" : "male";
        }
        /*write data to CSV file*/
        public static async Task<bool> WriteToCSV(IEnumerable<User> usersData, string filePath)
        {
            var csv = new StringBuilder();
            var header = "id,FirstName,LastName,Sex,BDate,Country,City,PM,MobilePhone,HomePhone,Time,Relation,Partner";
            csv.AppendLine(header);
            try
            {
                foreach (var user in usersData)
                {
                    if (user != null)
                    {
                        string[] row = user.GetValues();
                        csv.AppendLine(string.Join(",", row));
                    }
                }

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /*returns array of group`s ids from it`s urls*/
        public static async Task<string[]> GroupUrlToId(string[] urls)
        {
            List<string> group_ids = new List<string>();
            foreach (var url in urls)
            {
                group_ids.Add(url.Split('/').Last());
            }
            List<string> result = new List<string>();

            try
            {
                foreach (var id in group_ids)
                {
                    JObject group = await API.groups.getById(id, null);
                    if (group["response"] != null)
                    {
                        string respID = (((JArray)group["response"]).Children<JObject>().FirstOrDefault(item => item["screen_name"] != null && item["screen_name"].ToString() == id))["id"].ToString();
                        if (respID != null)
                            result.Add(respID);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            return result.ToArray();
        }
    }
}
