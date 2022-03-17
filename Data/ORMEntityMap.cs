using System;

namespace CyberBalance.CS.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ORMEntityMap : Attribute
    {
        #region PROPS  
        public string TableName { get; set; }
        public string CrudSprocName { get; set; }
        #endregion

        #region CTOR 
        public ORMEntityMap()
        {
        }
        public ORMEntityMap(string TableName)
        {
            this.TableName = TableName;
        }
        #endregion
    }
}
