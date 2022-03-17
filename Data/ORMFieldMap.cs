using System;

namespace CyberBalance.CS.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ORMFieldMap : Attribute
    {
        #region PROPS  
        public string ColumnName { get; set; }
        public string SprocParamName { get; set; }
        #endregion

        #region CTOR  
        public ORMFieldMap()
        {
        }
        public ORMFieldMap(string FieldName)
        {
            this.ColumnName = FieldName;
            this.SprocParamName = FieldName;

        }
        #endregion
    }
}
