
using System;
using System.Collections.Generic; 
namespace CyberScope.Tests
{
    [EntityMeta("fsma_QuestionTypes")]
    public class QuestionType
    {
        public QuestionType(){ 
        
        } 
		public int PK_QuestionTypeId { get; set; }
		public string code { get; set; }
		public string description { get; set; }
		public string AgencyAnswer { get; set; }
		public int FK_QuestionEntryType { get; set; }
		public string SprocValidation { get; set; }
		public string AnswerTable { get; set; }
		public int FK_FormType { get; set; }
		public string PK_Org_Column { get; set; }
		public string PK_Comp_Column { get; set; }
    } 
}
    