using TinyCsvParser.Mapping;

namespace CyberScope.Tests.Selenium
{
    public class CsvInitializeAttemptMapping : CsvMapping<InitializeAttempt>
    {
        public CsvInitializeAttemptMapping() : base()
        {
            MapProperty(0, x => x.Section); 
        }
    }
}
