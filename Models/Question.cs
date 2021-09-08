
using System;
using System.Collections.Generic; 
namespace CyberScope.Tests
{
    [EntityMeta("fsma_Questions")]
    public class Question
    {
        public Question(){ 
        
        } 
		public QuestionType QuestionType { get; set; }
		public int PK_Question { get; set; }
		public string FormName { get; set; }
		public int QuestionNum { get; set; }
		public string Section { get; set; }
		public int FK_QuestionGroup { get; set; }
		public int PK_Question_Prev { get; set; }
		public string QuestionText { get; set; }
		public string QuestionFormText { get; set; }
		public string Table_Name { get; set; }
		public string Column_Question { get; set; }
		public int FK_Question_Picklist { get; set; }
		public int FK_PickListType { get; set; }
		public int FK_QuestionType { get; set; }
		public int sortpos { get; set; }
		public string reportable { get; set; }
		public int FK_InputType { get; set; }
		public string identifier_text { get; set; }
		public string selector { get => $"qid_{identifier_text.Replace(".", "_")}"; }
		public string help_text { get; set; }
		public int FK_PickList { get; set; }
		public int WarningQuestion { get; set; }
		public string ExternalLinkType { get; set; }
		public int PK_ExternalLink { get; set; }
		public int FK_QuestionTypeMaster { get; set; }
    } 
}
    