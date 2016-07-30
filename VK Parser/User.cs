using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VK_Parser
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("sex")]
        public string Sex { get; set; }
        [JsonProperty("bdate")]
        public string BDate { get; set; }
        [JsonProperty("country.title")]
        public string Country { get; set; }
        [JsonProperty("city.title")]
        public string City { get; set; }
        [JsonProperty("can_write_private_message")]
        public string PrivateMessage { get; set; }
        [JsonProperty("mobile_phone")]
        public string MobilePhone { get; set; }
        [JsonProperty("home_phone")]
        public string HomePhone { get; set; }
        [JsonProperty("last_seen.time")]
        public string Time { get; set; }
        [JsonProperty("relation")]
        public string Relation { get; set; }
    }

}
