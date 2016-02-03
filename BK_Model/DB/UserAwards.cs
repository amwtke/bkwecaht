
namespace BK.Model.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserAwards
    {
        public string AwardsName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectRemark { get; set; }
        public Nullable<System.DateTime> GetDate { get; set; }
        [Key]
        public long Id { get; set; }
        public System.Guid AccountEmail_uuid { get; set; }
    }
}
