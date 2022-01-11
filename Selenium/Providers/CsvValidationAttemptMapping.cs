using System.Collections.Generic;
using TinyCsvParser.Mapping;

namespace CyberScope.Tests.Selenium
{
    public class CsvValidationAttemptMapping : CsvMapping<ValidationAttempt>
    {
        public CsvValidationAttemptMapping() : base()
        {
            MapProperty(0, x => x.Section);
            MapProperty(1, x => x.MetricXpath);
            MapProperty(2, x => x.ErrorAttemptExpression);
            MapProperty(3, x => x.ExpectedError); 
        }
    }
    public class ProcessMap 
    { 
        public string ElementSelector { get; set; }
        public string Action { get; set; }
        public string Param { get; set; } 
    }
    public class CsvGenericMapping : CsvMapping<ProcessMap>
    {
        public CsvGenericMapping() : base()
        { 
            MapProperty(0, x => x.ElementSelector);
            MapProperty(1, x => x.Action);
            MapProperty(2, x => x.Param);  
        }
    } 
}
