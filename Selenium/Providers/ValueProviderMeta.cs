using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium 
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ValueProviderMeta : Attribute
    {

        #region PROPS 
        private string refid;
        public string ReferenceId
        {
            get { return refid; }
            set { refid = value; }
        }
        private string controlref;
        public string ControlReference
        {
            get { return controlref; }
            set { controlref = value; }
        }
        private ControlReferenceAttribute refAttribute = ControlReferenceAttribute.ID;
        public ControlReferenceAttribute ReferenceAttribute
        {
            get { return refAttribute; }
            set { refAttribute = value; }
        }
        #endregion

        #region CTOR

        public ValueProviderMeta()
        {

        }
        public ValueProviderMeta(string ReferenceId, string ControlReference)
        {
            refid = ReferenceId;
            controlref = ControlReference;
        }

        #endregion

    }
}
