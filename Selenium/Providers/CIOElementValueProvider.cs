using OpenQA.Selenium;

namespace CyberScope.Tests.Selenium.Providers
{
    [ElementValueProviderMeta("//form[contains(@action, '_CIO_')]")]
    public class CIOElementValueProvider : ElementValueProvider, IElementValueProvider
    {
        public CIOElementValueProvider()
        { 
        }
        public override void Populate(DriverService ds)
        {
            ds.OpenTab();

            ds.ToSection((g => g.SectionText.Contains("S1")));
            base.Populate(ds);

            string m111 = ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0";
            string m112 = ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0";
            string m115 = ds.GetElementValue(By.XPath("//td/span[contains(text(), '1.1.5')]/../..//*[contains(@class, 'CustomControlValue')]")) ?? "0";
            SetMetric("1.1.1", m111);
            SetMetric("1.1.2", m112);
            SetMetric("1.1.5", m115);
            
            ds.CloseTab(); 

            ds.OpenTab(); 
            ds.ToSection((g => g.SectionText.Contains("S1B")));
            base.Populate(ds);  
            ds.CloseTab();

            base.Populate(ds); 
        }
    }
}
