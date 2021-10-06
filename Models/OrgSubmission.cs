
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace CyberScope.Tests
{
    [EntityMeta("fsma_OrgSubmissions")]
    public class OrgSubmission
    {
        public OrgSubmission(){ 
        
        } 
		public int PK_OrgSubmission { get; set; }
		public int PK_Component { get; set; }
		public string PK_Form { get; set; }
		public string Status_code { get; set; }
		public string OrgSub_Description { get; set; }
		public int FK_ReportingCycle_Component { get; set; }
		public string RMAOverrideStatus { get; set; }
		public int FK_PK_Component { get; set; }
		public int FK_LINK { get; set; }
    }
    internal static class OrgSubmissionProvider
    {
        #region PROPS

        #endregion
        #region Methods

        public static IEnumerable<OrgSubmission> GetBySession(string UserName, string PK_FORM)
        { 
            List<OrgSubmission> items = new List<OrgSubmission>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CAClientConnectionString"].ConnectionString))
            {
                const string sql = @"
                    SELECT ORG.* FROM fsma_OrgSubmissions ORG  
                    INNER JOIN fsma_ReportingCycle_Components RCC ON ORG.FK_ReportingCycle_Component=RCC.PK_ReportingCycle_Component
                    INNER JOIN fsma_ReportingCycles RC ON RCC.FK_ReportingCycle=RC.PK_ReportingCycle
                    INNER JOIN aspnet_Users U ON U.PK_Component=RCC.FK_Component
                    WHERE LoweredUserName = @UserName
                    AND PK_FORM = @PK_FORM
                    ";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                { 
                    cmd.Parameters.AddWithValue("@UserName", UserName.ToLower());
                    cmd.Parameters.AddWithValue("@PK_FORM", PK_FORM);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            OrgSubmission item = new OrgSubmission()
                            {
                                PK_OrgSubmission = Convert.ToInt32(rdr["PK_OrgSubmission"]),
                                FK_ReportingCycle_Component = Convert.ToInt32(rdr["FK_ReportingCycle_Component"]),
                                Status_code = rdr["Status_code"].ToString(),
                                PK_Form = rdr["PK_Form"].ToString()
                            };
                            items.Add(item);
                        }
                    }
                }
            }
            return items;
        }
        #endregion
    }
}
    