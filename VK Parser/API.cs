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
                if(_client == null)
                {
                    _client = new HttpClient();
                }
                return _client;
            }
        }

        public static string ApiDomain { get { return "vk.com"; } }
        public static string ClientId { get { return "2274003"; } }
        public static string ClientSecret { get { return "hHbZxrka2uZ6jB1inYsH"; } }
        public static string Version { get { return "5.53"; } }
        public static string Token { get; set; }
        



        public static async Task<string> Authorize(string login, string password, string captcha_sid, string captcha_key)
        {
            string requestUri = string.Format("https://oauth.vk.com/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}&v={4}&2fa_supported=1",ClientId, ClientSecret, login, password, Version);
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
            catch(Exception ex)
            {
                    Debug.WriteLine(ex.Message);
                    return null;
            }
        }
    }
}
