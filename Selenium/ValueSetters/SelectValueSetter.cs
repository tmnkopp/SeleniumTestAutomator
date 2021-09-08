﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CyberScope.Tests.Selenium
{
    internal class SelectValueSetter : BaseValueSetter, IValueSetter
    {
        public void SetValue(ChromeDriver driver, string ElementId)
        { 
            Element = driver.FindElement(By.CssSelector($"#{ElementId}"));
            SelectElement sel = new SelectElement(Element);
            if (sel.SelectedOption == null)
            {
                sel.SelectByIndex(sel.Options.Count - 1);
            }  
        }
    }
}