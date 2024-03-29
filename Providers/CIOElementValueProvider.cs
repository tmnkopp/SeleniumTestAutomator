using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Text.RegularExpressions;

namespace CyberScope.Automator.Providers
{
    [ElementValueProviderMeta("//form[contains(@action, '_CIO_')]")]
    public class CIOElementValueProvider : ElementValueProvider, IElementValueProvider
    {
        private ChromeDriver driver;
        public CIOElementValueProvider()
        { 
        }
        public override void Populate(SessionContext ctx)
        {
            this.driver = ctx.Driver;  
            SetMetric("1.1.1T", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]"));
            SetMetric("1.1.1H", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_High')]"));
            SetMetric("1.1.1M", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_Moderate')]"));
            SetMetric("1.1.1L", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_Low')]"));
            
            SetMetric("1.1.2T", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]"));
            SetMetric("1.1.2H", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblHigh_Second')]"));
            SetMetric("1.1.2M", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblModerate_Second')]"));
            SetMetric("1.1.2L", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblLow_Second')]"));

            string m115 = _getElementValue("//td/span[contains(text(), '1.1.5')]/../..//*[contains(@class, 'CustomControlValue')]");
            SetMetric("1.1.5", m115);

            foreach (var key in ctx.Defaults.Keys.ToArray())
                ctx.Defaults[key] = ctx.Defaults[key].Contains("{") ? base.Eval<string>(ctx.Defaults[key]) : ctx.Defaults[key]; 
        } 
        private string _getElementValue(string XPATH){
            var elements = this.driver.FindElements(By.XPath(XPATH));
            if(elements.Count > 0) 
                return elements[0].Text;
            return "0";
        }
    }
}