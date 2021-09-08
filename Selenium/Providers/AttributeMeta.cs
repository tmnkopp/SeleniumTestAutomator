using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests.Selenium 
{
    #region ENUMS 
    public enum ControlReferenceAttribute
    {
        ID,
        NAME,
        CLASS,
    } 
    #endregion

    [AttributeUsage(AttributeTargets.Class)]
    public class CompositeElement : Attribute
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
        #endregion

        #region CTOR

        public CompositeElement()
        {

        }
        public CompositeElement(string ReferenceId, string ControlReference)
        {
            refid = ReferenceId;
            controlref = ControlReference;
        }

        #endregion

    }
}
