
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class RecruitResearchField
    {
        [Key]
        public long ID { get; set; }
        public Nullable<long> RecruitID { get; set; }
        public Nullable<long> ResearchFieldId { get; set; }
        public Nullable<long> SubResearchFieldId { get; set; }
    }
}
