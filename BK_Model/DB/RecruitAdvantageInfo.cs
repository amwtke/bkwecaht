
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class RecruitAdvantageInfo
    {
        [Key]
        public long ID { get; set; }
        public string Info { get; set; }
    }
}
