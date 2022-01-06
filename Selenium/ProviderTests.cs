using CyberScope.Data.Tests;
using Dapper;
using Dapper.Mapper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
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
        [Theory]
        [InlineData("2020-A-HVA")]
        public void OrgSubmissionProvider_Provides(string PK_Form)
        {
            var user = $"{ConfigurationManager.AppSettings.Get($"{UserContext.Agency.ToString()}User")}";
            var orgsub = OrgSubmissionProvider.GetBySession(user, PK_Form).FirstOrDefault();
            Assert.NotNull(orgsub);
        }
        [Theory]
        [InlineData("2021-A-IG")]
        public void MetricProvider_Provides(string PK_Form)
        {  
            var user = $"{ConfigurationManager.AppSettings.Get($"{UserContext.Agency.ToString()}User")}";

            var orgsub = OrgSubmissionProvider.GetBySession($"{user}", $"{PK_Form}").FirstOrDefault();  
            var metrics = MetricProvider.GetItems(orgsub);

            var result = (from m in metrics
                         where m.FsmaAnswer.Answer != null
                         select m).ToList();

            Assert.NotNull(result);
        }
        public class TestMethodInfo : MethodInfo
        {
            public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

            public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

            public override MethodAttributes Attributes => throw new NotImplementedException();

            public override string Name => throw new NotImplementedException();

            public override Type DeclaringType => throw new NotImplementedException();

            public override Type ReflectedType => throw new NotImplementedException();

            public override MethodInfo GetBaseDefinition()
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetParameters()
            {
                throw new NotImplementedException();
            }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }
        }
    }
}
