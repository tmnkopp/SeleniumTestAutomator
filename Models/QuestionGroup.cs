
using System;
using System.Collections.Generic; 
namespace CyberScope.Tests
{
    [EntityMeta("fsma_QuestionGroups")]
    public class QuestionGroup
    {
        public QuestionGroup(){
            Questions = new List<Question>();
        }
        public List<Question> Questions { get; set; }
        public int PK_QuestionGroup { get; set; }
		public string PK_Form { get; set; }
		public string GroupName { get; set; }
		public string Text { get; set; }
		public string SectionText { get; set; }
		public int FK_FormPage { get; set; }
		public int sortpos { get; set; }
    } 
}
    