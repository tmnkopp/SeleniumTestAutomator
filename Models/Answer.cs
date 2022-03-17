
using System;
using System.Collections.Generic; 
namespace CyberScope.Tests
{
    [ORMEntityMap("fsma_Answers")]
    public class FsmaAnswer
    {
        public FsmaAnswer(){ 
        
        } 
		public int PK_Answer { get; set; }
		public int FK_Question { get; set; }
		public int FK_OrgSubmission { get; set; }
		public string Answer { get; set; } 
		public int PK_QuestionType_MultiAnswer { get; set; }
		public string Answer2 { get; set; }
		public int FK_PK_Answer { get; set; }
    } 
}
    