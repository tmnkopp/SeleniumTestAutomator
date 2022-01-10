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
    public class GenericMap 
    {
        public string ColA { get; set; }
        public string ColB { get; set; }
        public string ColC { get; set; }
        public string ColD { get; set; }
        public string ColE { get; set; } 
    }
    public class CsvGenericMapping : CsvMapping<GenericMap>
    {
        public CsvGenericMapping() : base()
        {
            MapProperty(0, x => x.ColA);
            MapProperty(1, x => x.ColB);
            MapProperty(2, x => x.ColC);
            MapProperty(3, x => x.ColD);
            MapProperty(4, x => x.ColE);  
        }
    } 
}
