
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class ExperimentProject
    {
        public string Account { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string AnnexPath { get; set; }
        [Key]
        public long Id { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<int> Status { get; set; }
    }
}
