using Serilog;
using Xunit.Abstractions;
using CyberScope.Tests.Selenium.DataCall.Tests;

namespace CyberScope.Tests.Selenium.Datacall.Tests
{
    public abstract class BaseDataCallTest
    {
        protected ILogger _logger;
        protected ITestOutputHelper output;
        public BaseDataCallTest(ITestOutputHelper output)
        {
            this.output = output; 
            _logger = TestInitializer.InitLogging(output);
            TestInitializer.InitIIS();
        }
    }
}
