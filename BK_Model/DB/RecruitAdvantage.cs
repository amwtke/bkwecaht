
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class RecruitAdvantage
    {
        [Key]
        public long ID { get; set; }
        public Nullable<long> RecruitID { get; set; }
        public Nullable<long> AdvantageID { get; set; }
    }
}
