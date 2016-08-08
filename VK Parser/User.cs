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
        
        public string Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Sex { get; set; }
        
        public string BDate { get; set; }
        
        public string Country { get; set; }
        
        public string City { get; set; }
        
        public string PrivateMessage { get; set; }
        
        public string MobilePhone { get; set; }
        
        public string HomePhone { get; set; }
        
        public string Time { get; set; }
        
        public string Relation { get; set; }

        public string Partner { get; set; }
    }

}
