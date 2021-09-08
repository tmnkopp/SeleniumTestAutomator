
using System;
using System.Collections.Generic; 
namespace CyberScope.Tests
{
    [EntityMeta("fsma_ReportingCycles")]
    public class ReportingCycle
    {
        public ReportingCycle(){ 
        
        }
		public FormMaster FormMaster { get; set; }
		public int PK_ReportingCycle { get; set; }
		public string IntervalCode { get; set; }
		public string Period { get; set; }
		public string Year { get; set; }
		public string Description { get; set; }
		public DateTime DateModified { get; set; }
		public string IsActive { get; set; }
		public string Status { get; set; }
		public DateTime ScheduledActivation { get; set; }
		public DateTime ActualActivation { get; set; }
		public DateTime ScheduledClosed { get; set; }
		public DateTime ActualClosed { get; set; }
		public string Notes { get; set; }
		public string SplashHeader { get; set; }
		public int PK_DataCall { get; set; }
    } 
}
    