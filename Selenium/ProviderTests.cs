using Dapper;
using Dapper.Mapper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
namespace CyberScope.Tests.Selenium
{
    public class ProviderTests
    {
        [Theory] 
        [InlineData("CSTargerUrl")]
        [InlineData("AgencyUser")]
        [InlineData("AgencyPass")]
        [InlineData("AdminUser")]
        [InlineData("AdminPass")]
        public void UserSettingsProvider_Provides(string setting) { 
            Assert.NotNull(ConfigurationManager.AppSettings.Get(setting));
        } 
        [Fact]
        public void ConfigProvider_Provides()
        { 
            Assert.NotNull(SettingsProvider.InputDefaults);
            string dkey = "CIO";
            Dictionary<string, string> InputDefaults = SettingsProvider.InputDefaults[$"Global"];
            if (SettingsProvider.InputDefaults.ContainsKey($"{dkey}"))
            {
                foreach (var item in SettingsProvider.InputDefaults[$"{dkey}"])
                {
                    if (InputDefaults.ContainsKey(item.Key))
                        InputDefaults[item.Key] = item.Value;
                    else
                        InputDefaults.Add(item.Key, item.Value);
                }
            }
            Assert.NotNull(InputDefaults);
        }
        [Fact]
        public void ControlLocatorsProvider_Provides()
        {
            var set = SettingsProvider.ControlLocators[0];
            Assert.IsType<ControlLocator>(set);
        } 
    } 
}
