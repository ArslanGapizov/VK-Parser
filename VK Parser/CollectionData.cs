using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_Parser
{
    /*Collections for objectdataprovider and comboboxes*/
    public static class CollectionData
    {
        public static async Task<Dictionary<string, string>> CollectionContries()
        {
            dynamic countriesResponse = await API.database.getCountries("1", null, null, "1000");
            Dictionary<string, string> countries = new Dictionary<string, string>();
            foreach (var item in countriesResponse.response.items)
            {
                countries.Add(item.id.ToString(), item.title.ToString());
            }

            return countries;
        }
        public static async Task<Dictionary<string, string>> CollectionCities(string countryID)
        {
            dynamic citiesResponse = await API.database.getCites(countryID, null, null, "0", null, "1000");
            Dictionary<string, string> cities = new Dictionary<string, string>();
            object lockMe = new object();
            foreach (var item in citiesResponse.response.items)
            {
                cities.Add(item.id.ToString(), item.title.ToString());
            }

            return cities;
        }
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
