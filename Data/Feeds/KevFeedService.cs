using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberBalance.CS.Core 
{
    public class KevFeedService
    {
        public void FetchAndUpdateCVEs()
        {
            string feedUri = ConfigurationManager.AppSettings.Get($"CISA_CVE_FEED") ?? "https://www.cisa.gov/sites/default/files/feeds/known_exploited_vulnerabilities.json";
            var provider = new FeedProvider<CisaKevFeed>(feedUri);
            var feedItems = provider.GetFeed.Items;

            var service = new BatchInserter<KevFeedCve>()
                 .SetImportTable("##CVEs_CACHE")
                 .SetImportSproc("CISA_CVE_CRUD")
                 .WithEvents();
            service.BatchInsert(feedItems); 
        }
    }
}
