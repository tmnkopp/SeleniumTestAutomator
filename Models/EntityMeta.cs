using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Tests 
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityMeta : Attribute
    {

        #region PROPS 
        private string tablename;
        public string TableName
        {
            get { return tablename; }
            set { tablename = value; }
        }  
        #endregion

        #region CTOR

        public EntityMeta()
        {

        }
        public EntityMeta( string TableName)
        {
            tablename = TableName; 
        }

        #endregion

    }
    public static class PK_FORM
    {
        public static string BOD2201_Q4_2021 = "2021-Q4-BOD2201";
        public static string ED2101A_A_2021 = "2021-A-ED2101A";
        public static string ED2101N_A_2021 = "2021-A-ED2101N";
        public static string EINSTEIN_A_2021 = "2021-A-EINSTEIN";
        public static string HVA_A_2021 = "2021-A-HVA";
        public static string IG_A_2021 = "2021-A-IG";
        public static string SAOP_A_2021 = "2021-A-SAOP";
        public static string CIO_Q1_2021 = "2021-Q1-CIO";
        public static string RMA_Q1_2021 = "2021-Q1-RMA";
        public static string BODVDP_Q2_2021 = "2021-Q2-BODVDP";
        public static string CIO_Q2_2021 = "2021-Q2-CIO";
        public static string CYBEREO_Q2_2021 = "2021-Q2-CYBEREO";
        public static string RMA_Q2_2021 = "2021-Q2-RMA";
        public static string CIO_Q3_2021 = "2021-Q3-CIO";
        public static string RMA_Q3_2021 = "2021-Q3-RMA";
        public static string CIO_Q4_2021 = "2021-Q4-CIO";
        public static string RMA_Q4_2021 = "2021-Q4-RMA";
        public static string AAPS_A_2020 = "2020-A-AAPS";
        public static string HVA_A_2020 = "2020-A-HVA";
        public static string HVAPOAM_A_2020 = "2020-A-HVAPOAM";
        public static string IG_A_2020 = "2020-A-IG";
        public static string SAOP_A_2020 = "2020-A-SAOP";
        public static string CIO_Q1_2020 = "2020-Q1-CIO";
        public static string RMA_Q1_2020 = "2020-Q1-RMA";
        public static string CIO_Q2_2020 = "2020-Q2-CIO";
        public static string RMA_Q2_2020 = "2020-Q2-RMA";
    }
}
