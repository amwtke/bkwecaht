
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Recruit
    {
        [Key]
        public long ID { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<int> Quota { get; set; }
        public Nullable<int> Gender { get; set; }
        public Nullable<int> Mode { get; set; }
        public Nullable<int> EducationalRequirements { get; set; }
        public string Direction { get; set; }
        public string Requirement { get; set; }
        public string Subsidy { get; set; }
        public Nullable<System.DateTime> PublicDate { get; set; }
        public Nullable<int> IsOld { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
