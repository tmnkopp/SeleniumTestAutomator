
using System;
using System.Collections.Generic; 
namespace CyberScope.Tests
{
    [ORMEntityMap("fsma_FormMaster")]
    public class FormMaster
    {
        public FormMaster(){
			QuestionGroups = new List<QuestionGroup>();
		}
		public List<QuestionGroup> QuestionGroups { get; set; }
		public int FK_ReportingCycle { get; set; }
		public string PK_Form { get; set; }
		public string Report_Year { get; set; }
		public string Form_Description { get; set; }
		public string InternalForm { get; set; }
		public string TypeCode { get; set; }
		public string IntervalCode { get; set; }
		public string Period { get; set; } 
		public int FK_FormType { get; set; }
		public string CrystalReportForm { get; set; }
		public string CrystalReportForm2 { get; set; }
		public string SprocFormValidation { get; set; }
		public string SprocFormUpdate { get; set; }
		public string SprocFormSubmit { get; set; }
		public string CrystalReportForm3 { get; set; }
		public string SprocAppendix { get; set; }
    } 
}
    