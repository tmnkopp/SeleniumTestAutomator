using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium.DataCall.Tests
{
    public static class TestInitializer
    { 
        public static void InitIIS()
        {
            var CSTargerUrl = ConfigurationManager.AppSettings.Get("CSTargerUrl");
            if (Regex.IsMatch(CSTargerUrl, $"57236"))
            {
                var solutionFolder = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));
                var applicationPath = Path.Combine(solutionFolder, "CyberScope.Tests");
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                Process _iisProcess = new Process();
                _iisProcess.StartInfo.FileName = programFiles + @"\IIS Express\iisexpress.exe";
                _iisProcess.StartInfo.Arguments = string.Format("/path:{0} /port:{1}", applicationPath, "57236");
                _iisProcess.Start();
            } 
        } 
    }
}
