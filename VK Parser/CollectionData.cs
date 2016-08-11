using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_Parser
{
    /*Collections for objectdataprovider*/
    public static class CollectionData
    {
        public static Dictionary<string, string> CollectionSex()
        {
            Dictionary<string, string> sexDictionary = new Dictionary<string, string> { { "1", "female" }, { "2", "male" }, { "0", "any" } };
            return sexDictionary;
        }
        public static Dictionary<string, string> CollectionRelationshipStatus()
        {
            Dictionary<string, string> RelationDictionary = new Dictionary<string, string> { { "1", "Not married" }, { "2", "In relationship" }, { "3", "Engaged" }, { "4", "Married" }, { "5", "It`s complicated" }, { "6", "Actively searching" }, { "7", "In love" } };
            return RelationDictionary;
        }
    }
}
