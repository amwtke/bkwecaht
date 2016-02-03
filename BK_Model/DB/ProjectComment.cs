
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ProjectComment
    {
        [Key]
        public long Id { get; set; }
        public string UserAccount { get; set; }
        public string ServiceAccount { get; set; }
        public string CommentInfo { get; set; }
        public string ProjectName { get; set; }
        public Nullable<long> SolutionId { get; set; }
        public Nullable<int> Grade { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
    }
}
