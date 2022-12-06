using OpenQA.Selenium;
namespace CyberScope.Automator.Providers
{
    [ElementValueProviderMeta("//form[contains(@action, '_CIO_')]")]
    public class CIOElementValueProvider : ElementValueProvider, IElementValueProvider
    {
        public CIOElementValueProvider()
        { 
        }
        public override void Populate(CsDriverService ds)
        {
            ds.OpenTab();
            ds.ToSection((g => g.SectionText.Contains("S1")));
            base.Populate(ds);
       
            string m115 = ds.GetElementValue(By.XPath("//td/span[contains(text(), '1.1.5')]/../..//*[contains(@class, 'CustomControlValue')]")) ?? "0";
            
            SetMetric("1.1.1", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Total')]")) ?? "0");
            SetMetric("1.1.1H", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_High')]")) ?? "0");
            SetMetric("1.1.1M", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Moderate')]")) ?? "0");
            SetMetric("1.1.1L", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblfirst_Low')]")) ?? "0");
            SetMetric("1.1.2", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblSecond_Total')]")) ?? "0");
            SetMetric("1.1.2H", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblHigh_Second')]")) ?? "0");
            SetMetric("1.1.2M", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblModerate_Second')]")) ?? "0");
            SetMetric("1.1.2L", ds.GetElementValue(By.XPath("//tr[last()]/td/span[contains(@id, 'lblLow_Second')]")) ?? "0");
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