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

        public string Skype { get; set; }

        public string Instagram { get; set; }

        public string HomePhone { get; set; }

        public DateTime Time { get; set; }

        public string Relation { get; set; }

        public string Partner { get; set; }

        /*Returns array of values*/
        public string[] GetValues()
        {
            string[] values =
            {
                string.Format("\"{0}\"", Id),
                string.Format("\"{0}\"", FirstName),
                string.Format("\"{0}\"", LastName),
                string.Format("\"{0}\"", Sex),
                string.Format("\"{0}\"", BDate),
                string.Format("\"{0}\"", Country),
                string.Format("\"{0}\"", City),
                string.Format("\"{0}\"", PrivateMessage),
                string.Format("\"{0}\"", MobilePhone),
                string.Format("\"{0}\"", Skype),
                string.Format("\"{0}\"", Instagram),
                string.Format("\"{0}\"", HomePhone),
                string.Format("\"{0}\"", Time.ToString("yyyy-MM-dd HH:mm:ss")),
                string.Format("\"{0}\"", Relation),
                string.Format("\"{0}\"", Partner)
            };
            return values;
        }
    }

}
