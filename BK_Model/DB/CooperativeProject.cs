
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class CooperativeProject
    {
        [Key]
        public long ID { get; set; }
        public Nullable<System.DateTime> PublicDate { get; set; }
        public Nullable<int> NumOfRead { get; set; }
        public string Title { get; set; }
        public string Reward { get; set; }
        public string Achievement { get; set; }
        public string Summary { get; set; }
        public Nullable<System.DateTime> Deadline { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
