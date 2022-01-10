﻿using TinyCsvParser.Mapping;

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
}
