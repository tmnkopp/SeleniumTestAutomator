using System;

namespace CyberScope.Tests.Selenium
{
    [AttributeUsage(System.AttributeTargets.Class)]
    public class ConnectorMeta : Attribute
    {
        public string Selector { get; set; }
        public ConnectorMeta(string Selector)
        {
            this.Selector = Selector;
        }
    }

}
