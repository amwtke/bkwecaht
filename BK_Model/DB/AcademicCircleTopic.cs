
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class AcademicCircleTopic
    {
        [Key]
        public long ID { get; set; }
        public string Title { get; set; }
        public string BodyText { get; set; }
        public Nullable<System.DateTime> PublicDate { get; set; }
        public Nullable<int> NumOfRead { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
