
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class RecruitSignUp
    {
        [Key]
        public long ID { get; set; }
        public Nullable<long> RecruitID { get; set; }
        public Nullable<System.DateTime> PublicDate { get; set; }
        public string Detail { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> Contact { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
