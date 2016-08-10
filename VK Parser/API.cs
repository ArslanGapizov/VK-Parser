using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

namespace VK_Parser
{
    public static class API
    {

        private static HttpClient _client;

        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                }
                return _client;
            }
        }

        public static string Lang { get; set; } = "0";
        public static string ApiDomain { get { return "api.vk.com"; } }
        public static string ClientId { get { return "2274003"; } }
        public static string ClientSecret { get { return "hHbZxrka2uZ6jB1inYsH"; } }
        public static string Version { get { return "5.53"; } }
        public static string AccessToken { get; set; }

        public static async Task<string> RunAsync(string method, Dictionary<string, string> pars)
        {
            IEnumerable<KeyValuePair<string, string>> source = pars.Where((f => f.Value != null));
            pars = source.ToDictionary(x => x.Key, x => x.Value);
            pars.Add("access_token", AccessToken);
            pars.Add("lang", Lang);
            pars.Add("v", Version);
            string format = "/method/{0}?{1}";
            string[] paramArray = new string[2]
            {
                method,
                pars.Keys.Aggregate("",(current, s) => current + string.Format("{0}={1}", new object[2] { s, pars[s] }) + "&" )
            };

            string result = string.Format("https://" + ApiDomain + format, paramArray);
            Debug.WriteLine(result);
            return await (await GetAsync(result)).Content.ReadAsStringAsync();
        }


        public static async Task<string> Authorize(string login, string password, string captcha_sid, string captcha_key)
        {
            string requestUri = string.Format("https://oauth.vk.com/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}&v={4}&2fa_supported=1", ClientId, ClientSecret, login, password, Version);
            if (captcha_key != null)
                requestUri += string.Format("&captcha_sid={0}&captcha_key={1}", new object[2] { (object)captcha_sid, (object)captcha_key });

            return await (await GetAsync(requestUri)).Content.ReadAsStringAsync();
        }

        public static async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            try
            {
                return await Client.GetAsync(requestUri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public static class database
        {
            public static async Task<string> getCountries(string need_all, string code, string offset, string count)
            {
                return await RunAsync("database.getCountries", new Dictionary<string, string>
                {
                    { "need_all", need_all },
                    { "code", code },
                    { "offset", offset },
                    { "count", count }
                });
            }

            public static async Task<string> getRegions(string country_id, string q, string offset, string count)
            {
                return await RunAsync("database.getRegions", new Dictionary<string, string>
                {
                    { "country_id", country_id },
                    { "q", q },
                    { "offset", offset },
                    { "count", count }
                });
            }

            public static async Task<string> getCites(string country_id, string region_id, string q, string need_all, string offset, string count)
            {
                return await RunAsync("database.getCities", new Dictionary<string, string>
                {
                    { "country_id", country_id },
                    { "region_id", region_id },
                    { "q", q },
                    { "need_all", need_all },
                    { "offset", offset },
                    { "count", count }
                });
            }

            public static async Task<string> getUniversities(string q, string country_id, string city_id, string offset, string count)
            {
                return await RunAsync("database.getUniversities", new Dictionary<string, string>
                {
                    { "q", q },
                    { "country_id", country_id },
                    { "city_id", city_id },
                    { "offset", offset },
                    { "count", count }
                });
            }
        }

        public static class users
        {
            public static async Task<string> search(string country, string city, string university, string sex, string status, string count, string fields, DateTime date, string group_id)
            {
                return await RunAsync("users.search", new Dictionary<string, string>
                {
                    { "country", country },
                    { "city", city },
                    { "university", university },
                    { "sex", sex },
                    { "status", status },
                    { "count",  count},
                    { "fields", fields },
                    { "birth_day", date.Day.ToString() },
                    { "birth_month", date.Month.ToString() },
                    { "birth_year", date.Year.ToString() },
                    { "group_id",  group_id}
                });
            }
        }
    }
}
