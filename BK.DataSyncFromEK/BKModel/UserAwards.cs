
namespace BK.DataSyncFromEK.BKModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserAwards: IDBModelWithID
    {
        public string AwardsName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectRemark { get; set; }
        public DateTime? GetDate { get; set; }
        [Key]
        public long Id { get; set; }
        public Guid AccountEmail_uuid { get; set; }
    }
}
