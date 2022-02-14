using TinyCsvParser.Mapping;

namespace CyberScope.Tests.Selenium
{
    public class CsvGenericMapping : CsvMapping<ProcessMap>
    {
        public CsvGenericMapping() : base()
        { 
            MapProperty(0, x => x.ElementLocator);
            MapProperty(1, x => x.Action);
            MapProperty(2, x => x.Param);  
        }
    } 
}
