
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class qa_answer
    {
        [Key]
        public long id { get; set; }
        public Nullable<long> question_id { get; set; }
        public string answer_body { get; set; }
        public Nullable<System.DateTime> createtime { get; set; }
        public Nullable<int> up_hit { get; set; }
        public Nullable<int> down_hit { get; set; }
        public System.Guid email_uuid { get; set; }
    }
}
