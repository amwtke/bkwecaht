
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class CooperativeProjectPraise
    {
        [Key]
        public long ID { get; set; }
        public Nullable<long> ProjectID { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public int Praise { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
