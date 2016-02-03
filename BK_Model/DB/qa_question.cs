
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class qa_question
    {
        [Key]
        public long id { get; set; }
        public string question_title { get; set; }
        public string question_body { get; set; }
        public string question_type { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
        public string keywords { get; set; }
        public Nullable<int> readpoint { get; set; }
        public System.Guid email_uuid { get; set; }
    }
}
