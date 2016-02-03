
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Conference
    {
        [Key]
        public long ID { get; set; }
        public string Title { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public string Sponsor { get; set; }
        public string Instruction { get; set; }
        public string Homepage { get; set; }
        public string RegistrationLink { get; set; }
        public Nullable<int> Solicit { get; set; }
        public Nullable<int> NumOfRead { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
