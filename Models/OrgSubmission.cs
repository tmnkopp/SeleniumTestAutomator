
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
	public class Metric {
        public Metric()
        {
            OrgSubmission = new OrgSubmission();
            Question = new Question();
            FsmaAnswer = new FsmaAnswer();
        }
        public OrgSubmission OrgSubmission { get; set; }
        public Question Question { get; set; } 
        public FsmaAnswer FsmaAnswer { get; set; }

    }
}
    