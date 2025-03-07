using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
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
            base.OnPostPopulate += (sender, args) =>
            {
                var ctx = args.Context;
                var control = args.Driver.FindElements(By.XPath("//*[contains(@id, '_btnEdit')]"));
                this.PostPopulate(ctx);
            };
        }
        public void PostPopulate(SessionContext ctx)
        {  
            this.driver = ctx.Driver;
            string value = "";
            
            SetMetric("1.1.1T", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]"));
            SetMetric("1.1.1H", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_High')]"));
            SetMetric("1.1.1M", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_Moderate')]"));
            SetMetric("1.1.1L", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblfirst_Low')]"));
            SetMetric("1.1.2T", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]"));
            SetMetric("1.1.2H", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblHigh_Second')]"));
            SetMetric("1.1.2M", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblModerate_Second')]"));
            SetMetric("1.1.2L", _getElementValue("//tr[last()]/td/span[contains(@id, 'lblLow_Second')]"));
            SetMetric("1.1.5", _getElementValue("//td/span[contains(text(), '1.1.5')]/../..//*[contains(@class, 'CustomControlValue')]"));

            base.ApplyMetricsToContextDefaults(ctx);

        } 
        private string _getElementValue(string XPATH){
            var elements = this.driver.FindElements(By.XPath(XPATH));
            return (elements.Count > 0) ? elements[0].Text : null; 
        }
    }
}