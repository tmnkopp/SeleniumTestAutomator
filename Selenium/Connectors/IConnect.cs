using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
    public interface IConnect
    {
        void Connect(SessionContext context);
    } 
}
