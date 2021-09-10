using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium
{
   
    public class InvGrid : BaseAutomator, IAutomator 
    {
        #region PROPS
        private IWebElement ele;
        private WebDriverWait wait;
        #endregion
          
        #region CTOR
        public InvGrid()
        { 
        }
        #endregion

        #region METHODS
        public void Automate(ChromeDriver driver)
        {
            IReadOnlyCollection<IWebElement> elist =
                driver.FindElementsByXPath(@"//table[contains(@class,'rgMasterTable')]/tbody//tr[contains(@class,'Row')]");
            var ids = (from e in elist select e.GetAttribute("id")).ToList();
            foreach (string id in ids)
            { 
                var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//a[contains(text(), 'Reset')]");
                if (esublist.Count > 0)
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                    esublist[0].Click();
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
                    IAlert alert = driver.SwitchTo().Alert();
                    alert.Accept(); 
                }
            }
            foreach (string id in ids)
            { 
                var esublist = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@id, '_EditButton')]");
                if (esublist.Count > 0)
                {
                    esublist[0].Click(); 
                    var inputs = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//input[contains(@type, 'text')]");
                    foreach (var input in inputs)
                    {
                        if (input.GetAttribute("value") == "") 
                            input.SendKeys("1");  
                    }   
                } 
                var elements = driver.FindElements(By.CssSelector($"#{id} input[id*=_UpdateButton]"));
                if (elements.Count > 0) 
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elements[0]); 
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1); 
            }
            foreach (string id in ids)
            { 
                var elements = driver.FindElementsByXPath($"//tr[contains(@id, '{id}')]//a[contains(text(), 'Submit')]"); 
                if (elements.Count > 0)
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", elements[0]);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                    driver.SwitchTo().Alert().Accept();
                }
            }
        }
        #endregion 
    }
}
