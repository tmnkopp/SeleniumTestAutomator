using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberBalance.CS.Core
{
    public interface IFeed<T>
    { 
        IEnumerable<T> Items { get; set; }
        DateTime DateReleased { get; set; }
        string Version { get; set; }
    } 
    public class CisaKevFeed: IFeed<KevFeedCve>
    {
        public string title { get; set; }
        [JsonProperty("catalogVersion")]
        public string Version { get; set; }
        [JsonProperty("dateReleased")]
        public DateTime DateReleased { get; set; }
        public int count { get; set; }
        [JsonProperty("vulnerabilities")]
        public IEnumerable<KevFeedCve> Items { get; set; }
    }

    public class KevFeedCve 
    {
        [JsonProperty("cveID")]
        public string PK_CISA_CVE { get; set; }
        [JsonProperty("vendorProject")]
        public string Vendor { get; set; }
        [JsonProperty("product")]
        public string Product { get; set; }
        [JsonProperty("vulnerabilityName")]
        public string VulnerabilityName { get; set; }
        [JsonProperty("dateAdded")]
        public DateTime DateAdded { get; set; }
        [JsonProperty("shortDescription")]
        public string CVEDescription { get; set; }
        [JsonProperty("requiredAction")]
        public string Action { get; set; }
        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }
    }
}
